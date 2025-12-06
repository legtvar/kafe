using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Kafe.Media;
using System.Net.Http;
using System.Net.Http.Json;
using Kafe.Media.Services;

namespace Kafe.Api.Daemons;

public record PigeonTask(int Id, string Payload);

public class PigeonsTestQueueDaemon(
    IServiceProvider serviceProvider,
    IHttpClientFactory httpClientFactory,
    IPigeonsTestQueue PigeonsTestQueue,
    ILogger<PigeonsTestQueueDaemon> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var request = await PigeonsTestQueue.DequeueAsync(ct);
            if (request is null)
            {
                logger.LogInformation("No Pigeons test requests in queue. Waiting.");
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
                return;
            }
            var client = httpClientFactory.CreateClient("Pigeons");
            var response = await client.PostAsJsonAsync("/test", request, cancellationToken: ct);
            var content = await response.Content.ReadFromJsonAsync<BlendInfoJsonFormat>(cancellationToken: ct);
            if (content is null)
            {
                throw new InvalidOperationException("Failed to get pigeons test info from pigeons service.");
            }
            logger.LogInformation("Processed Pigeons test request for shard {ShardId}", request.ShardId);
        }
    }
}
