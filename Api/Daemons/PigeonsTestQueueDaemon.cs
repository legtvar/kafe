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

        var missingTestShardIds = await shardService.GetMissingTestBlends(ct);
        foreach (var shardId in missingTestShardIds)
        {
            try
            {
                await PigeonsTestQueue.EnqueueAsync(shardId);
                logger.LogInformation("Re-enqueued Pigeons test request for shard {ShardId}", shardId);
            }
            catch (Exception ex)
            {
                var blendInfo = new BlendInfo(
                    FileExtension: Const.BlendFileExtension,
                    MimeType: Const.BlendMimeType,
                    Tests: null,
                    Error: $"{ex?.Message}"
                );
                await shardService.UpdateBlend(shardId, blendInfo, ct);
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
            var shardId = await PigeonsTestQueue.DequeueAsync(ct);
            if (shardId is null)
            {
                logger.LogInformation("Pigeons test daemon: No Pigeons test requests in queue. Waiting.");
                return;
            }
            int attempt = 0;
            bool success = false;

            while (attempt < retryMaxAttempts && !success && !ct.IsCancellationRequested)
            {
                try
                {
                    var testResult = await shardService.TestBlend(shardId, ct);
                    if (testResult is null)
                    {
                        attempt = retryMaxAttempts;
                        throw new InvalidOperationException($"Failed to create Pigeons test for shard {shardId}.");
                    }
                    logger.LogInformation("Pigeons test daemon: Processed Pigeons test for shard {ShardId}", shardId);
                    success = true;
                }
                catch (Exception ex)
                {
                    attempt++;
                    if (attempt < retryMaxAttempts)
                    {
                        logger.LogWarning(ex, "Pigeons test daemon: Attempt {Attempt}/{MaxAttempts} failed for shard {ShardId}. Retrying in {Delay}s...", attempt, retryMaxAttempts, shardId, retryDelaySeconds);
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
                        await shardService.UpdateBlend(shardId, blendInfo, ct);
                        logger.LogError(ex, "Pigeons test daemon: All {MaxAttempts} attempts failed for shard {ShardId}. Giving up.", retryMaxAttempts, shardId);
                    }
                }
            }
        }
    }
}
