using System;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Options;
using Kafe.Data.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kafe.Api.Daemons;

public class VideoConversionDaemon(
    IServiceProvider serviceProvider,
    ILogger<VideoConversionDaemon> logger,
    IOptions<VideoConversionOptions> options
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var conversionService = scope.ServiceProvider.GetRequiredService<VideoConversionService>();
            var conversion = await conversionService.FindConversionToHandle(ct);
            if (conversion is null)
            {
                logger.LogInformation("Found no videos to convert. Waiting.");
                await Task.Delay(options.Value.PollWaitTime, ct);
                continue;
            }

            await conversionService.ConvertVideoPersist(conversion, ct);
        }
    }
}
