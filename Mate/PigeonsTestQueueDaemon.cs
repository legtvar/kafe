using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe.Mate;

public class PigeonsTestQueueDaemon(
    IServiceProvider serviceProvider,
    PigeonsTestQueue pigeonsQueue,
    ILogger<PigeonsTestQueueDaemon> logger
) : BackgroundService
{
    private const int RetryMaxAttempts = 3;
    private const int RetryDelaySeconds = 10;

    public override async Task StartAsync(CancellationToken ct)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var pigeonsService = scope.ServiceProvider.GetRequiredService<PigeonsService>();

        var missingTestShardIds = await pigeonsService.GetUntestedBlends(ct);
        foreach (var shardId in missingTestShardIds)
        {
            try
            {
                await pigeonsQueue.EnqueueAsync(shardId);
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
                await pigeonsService.UpdateBlend(shardId, blendInfo, ct);
            }
        }

        logger.LogInformation("Pigeons test daemon: Pigeons test queue daemon started.");
        await base.StartAsync(ct);
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var pigeonsService = scope.ServiceProvider.GetRequiredService<PigeonsService>();
        while (!ct.IsCancellationRequested)
        {
            var shardId = pigeonsQueue.Dequeue();
            if (shardId is null)
            {
                logger.LogInformation("Pigeons test daemon: No Pigeons test requests in queue. Waiting.");
                await pigeonsQueue.Wait(ct);
                return;
            }

            var attempt = 0;
            var success = false;

            while (attempt < RetryMaxAttempts && !success && !ct.IsCancellationRequested)
            {
                try
                {
                    var testResult = await pigeonsService.TestBlend(shardId, ct);
                    if (testResult is null)
                    {
                        attempt = RetryMaxAttempts;
                        throw new InvalidOperationException($"Failed to create Pigeons test for shard {shardId}.");
                    }

                    logger.LogInformation("Pigeons test daemon: Processed Pigeons test for shard {ShardId}", shardId);
                    success = true;
                }
                catch (Exception ex)
                {
                    attempt++;
                    if (attempt < RetryMaxAttempts)
                    {
                        logger.LogWarning(
                            ex,
                            "Pigeons test daemon: Attempt {Attempt}/{MaxAttempts} failed for shard {ShardId}. Retrying in {Delay}s...",
                            attempt,
                            RetryMaxAttempts,
                            shardId,
                            RetryDelaySeconds
                        );
                        await Task.Delay(TimeSpan.FromSeconds(RetryDelaySeconds), ct);
                    }
                    else
                    {
                        var blendInfo = new BlendInfo(
                            FileExtension: Const.BlendFileExtension,
                            MimeType: Const.BlendMimeType,
                            Tests: null,
                            Error: $"{ex?.Message}"
                        );
                        await pigeonsService.UpdateBlend(shardId, blendInfo, ct);
                        logger.LogError(
                            ex,
                            "Pigeons test daemon: All {MaxAttempts} attempts failed for shard {ShardId}. Giving up.",
                            RetryMaxAttempts,
                            shardId
                        );
                    }
                }
            }
        }
    }
}
