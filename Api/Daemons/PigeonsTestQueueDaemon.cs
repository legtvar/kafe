using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Kafe.Media;
using System.Net.Http;
using System.Net.Http.Json;
using Kafe.Media.Services;
using Kafe.Data.Services;
using Microsoft.Extensions.DependencyInjection;
using Kafe.Data;

namespace Kafe.Api.Daemons;

public class PigeonsTestQueueDaemon(
    IServiceProvider serviceProvider,
    IHttpClientFactory httpClientFactory,
    IPigeonsTestQueue PigeonsTestQueue,
    ILogger<PigeonsTestQueueDaemon> logger
) : BackgroundService
{
    private const int retryMaxAttempts = 3;
    private const int retryDelaySeconds = 10;

    public override async Task StartAsync(CancellationToken ct)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var artifactService = scope.ServiceProvider.GetRequiredService<ArtifactService>();
        var shardService = scope.ServiceProvider.GetRequiredService<ShardService>();
        var storageService = scope.ServiceProvider.GetRequiredService<StorageService>();

        var missingTests = await shardService.GetShardsMissingTestAsync(ct);
        foreach (var shard in missingTests)
        {
            try
            {
                var shard_exists = shardService.Load(shard.Id, ct);
                if (shard_exists == null)
                {
                    throw new InvalidOperationException($"Shard {shard.Id} no longer exists.");
                }
                var projectGroupNames = await artifactService.GetArtifactProjectGroupNames(shard.ArtifactId.ToString(), ct);
                if (projectGroupNames.Length != 1)
                {
                    throw new InvalidOperationException($"Shard {shard.Id} does not belong to any project.");
                }
                if (!storageService.TryGetFilePath(
                    ShardKind.Blend,
                    shard.Id,
                    Const.OriginalShardVariant,
                    out var shardFilePath))
                {
                    throw new InvalidOperationException($"Can't find shard path of {shard.Id}.");
                }
                var request = new PigeonsTestRequest(
                    ShardId: shard.Id,
                    HomeworkType: projectGroupNames[0]["iv"] ?? string.Empty,
                    Path: shardFilePath);

                await PigeonsTestQueue.EnqueueAsync(request);
                logger.LogInformation("Re-enqueued Pigeons test request for shard {ShardId}", shard.Id);
            }
            catch (Exception ex)
            {
                var blendInfo = new BlendInfo(
                    FileExtension: Const.BlendFileExtension,
                    MimeType: Const.BlendMimeType,
                    Tests: null,
                    Error: $"{ex?.Message}"
                );
                await shardService.UpdateBlendTest(shard.Id, blendInfo, ct);
            }
        }
        logger.LogInformation("Pigeons test daemon: Pigeons test queue daemon started.");
        await base.StartAsync(ct);
    }
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var shardService = scope.ServiceProvider.GetRequiredService<ShardService>();
        while (!ct.IsCancellationRequested)
        {
            var request = await PigeonsTestQueue.DequeueAsync(ct);
            if (request is null)
            {
                logger.LogInformation("Pigeons test daemon: No Pigeons test requests in queue. Waiting.");
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
                return;
            }

            int attempt = 0;
            bool success = false;

            while (attempt < retryMaxAttempts && !success && !ct.IsCancellationRequested)
            {
                try
                {
                    bool shardExists = await IsExistingShard(
                        request.ShardId,
                        shardService,
                        scope.ServiceProvider.GetRequiredService<ArtifactService>(),
                        scope.ServiceProvider.GetRequiredService<StorageService>(),
                        ct);
                    if (!shardExists)
                    {
                        attempt = retryMaxAttempts;
                        throw new InvalidOperationException($"Shard {request.ShardId} no longer exists.");
                    }

                    var client = httpClientFactory.CreateClient("Pigeons");
                    var response = await client.PostAsJsonAsync("/test", request, cancellationToken: ct);
                    var content = await response.Content.ReadFromJsonAsync<BlendInfoJsonFormat>(cancellationToken: ct);
                    if (content is null)
                    {
                        throw new InvalidOperationException("Failed to get pigeons test info from pigeons service.");
                    }

                    var blendInfo = content.ToBlendInfo();
                    if (await shardService.UpdateBlendTest(request.ShardId, blendInfo, ct) == null)
                    {
                        throw new InvalidOperationException($"Failed to update shard {request.ShardId} with pigeons test info.");
                    }
                    else
                    {
                        logger.LogInformation("Pigeons test daemon: Processed Pigeons test request for shard {ShardId}", request.ShardId);
                        success = true;
                    }
                }
                catch (Exception ex)
                {
                    attempt++;
                    if (attempt < retryMaxAttempts)
                    {
                        logger.LogWarning(ex, "Pigeons test daemon: Attempt {Attempt}/{MaxAttempts} failed for shard {ShardId}. Retrying in {Delay}s...", attempt, retryMaxAttempts, request.ShardId, retryDelaySeconds);
                        await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), ct);
                    }
                    else
                    {
                        var blendInfo = new BlendInfo(
                            FileExtension: Const.BlendFileExtension,
                            MimeType: Const.BlendMimeType,
                            Tests: null,
                            Error: $"{ex?.Message}"
                        );
                        await shardService.UpdateBlendTest(request.ShardId, blendInfo, ct);
                        logger.LogError(ex, "Pigeons test daemon: All {MaxAttempts} attempts failed for shard {ShardId}. Giving up.", retryMaxAttempts, request.ShardId);
                    }
                }
            }
        }
    }

    private async Task<bool> IsExistingShard(
        string shardId,
        ShardService shardService,
        ArtifactService artifactService,
        StorageService storageService,
        CancellationToken ct)
    {
        var shard = await shardService.Load(shardId, ct);
        if (shard == null)
        {
            return false;
        }
        var projectGroupNames = await artifactService.GetArtifactProjectGroupNames(shard.ArtifactId.ToString(), ct);
        if (projectGroupNames.Length < 1)
        {
            return false;
        }
        if (!storageService.TryGetFilePath(
            ShardKind.Blend,
            shard.Id,
            Const.OriginalShardVariant,
            out var shardFilePath))
        {
            return false;
        }
        return true;
    }
}
