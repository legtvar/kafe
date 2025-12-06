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
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var shardService = scope.ServiceProvider.GetRequiredService<ShardService>();

            var request = await PigeonsTestQueue.DequeueAsync(ct);
            if (request is null)
            {
                logger.LogInformation("No Pigeons test requests in queue. Waiting.");
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
                return;
            }

            int attempt = 0;
            bool success = false;
            Exception? lastException = null;

            while (attempt < retryMaxAttempts && !success && !ct.IsCancellationRequested)
            {
                try
                {
                    var client = httpClientFactory.CreateClient("Pigeons");
                    var response = await client.PostAsJsonAsync("/test", request, cancellationToken: ct);
                    var content = await response.Content.ReadFromJsonAsync<BlendInfoJsonFormat>(cancellationToken: ct);
                    if (content is null)
                    {
                        throw new InvalidOperationException("Failed to get pigeons test info from pigeons service.");
                    }

                    var blendInfo = content.ToBlendInfo();
                    if (await shardService.UpdateBlend(request.ShardId, blendInfo, ct) == null)
                    {
                        logger.LogWarning("Failed to update shard {ShardId} with pigeons test info.", request.ShardId);
                        throw new InvalidOperationException($"Failed to update shard {request.ShardId} with pigeons test info.");
                    }
                    else
                    {
                        logger.LogInformation("Processed Pigeons test request for shard {ShardId}", request.ShardId);
                        success = true;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    attempt++;
                    if (attempt < retryMaxAttempts)
                    {
                        logger.LogWarning(ex, "Attempt {Attempt}/{MaxAttempts} failed for shard {ShardId}. Retrying in {Delay}s...", attempt, retryMaxAttempts, request.ShardId, retryDelaySeconds);
                        await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), ct);
                    }
                    else
                    {
                        logger.LogError(ex, "All {MaxAttempts} attempts failed for shard {ShardId}. Giving up.", retryMaxAttempts, request.ShardId);
                    }
                }
            }
        }
    }
}
