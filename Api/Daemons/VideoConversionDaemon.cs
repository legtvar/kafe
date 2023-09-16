using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Aggregates;
using Kafe.Media;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kafe.Api.Daemons;

public class VideoConversionDaemon : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<VideoConversionDaemon> logger;

    public VideoConversionDaemon(
        IServiceProvider serviceProvider,
        ILogger<VideoConversionDaemon> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var video = await FindVideoToConvert();
        }
    }

    private async Task<VideoConversionInfo?> FindVideoToConvert()
    {
        using var scope = serviceProvider.CreateAsyncScope();
        using var db = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
        var unfinishedConversions = await db.Query<VideoConversionInfo>()
            .Where(i => !i.IsCompleted && !i.HasFailed)
            .ToListAsync();
        if (unfinishedConversions.Count > 0)
        {
            var conversion = unfinishedConversions[0];
            logger.LogDebug(
                "Found unfinished conversion '{}' of video '{}' to variant '{}'.",
                conversion.Id,
                conversion.VideoId,
                conversion.Variant);
            return conversion;
        }
        
        var today = DateTimeOffset.UtcNow.Date;
        var videosOfToday = await db.Query<VideoShardInfo>()
            .Where(v => ((string)(object)v.CreatedAt).StartsWith(today.ToString("yyyy-MM-dd")))
            .ToListAsync();

        return null;
    }

    private static ImmutableArray<string> GetMissingVariants(VideoShardInfo video)
    {
        if (!video.Variants.TryGetValue(Const.OriginalShardVariant, out var originalVariant))
        {
            throw new ArgumentException($"VideoShhard '{video.Id}' is missing the 'original' variant.");
        }
        
        var desiredVariants = Video.GetApplicablePresets(originalVariant);
        var missingVariants = video.Variants.Keys
            .Except(desiredVariants.Select(p => p.ToFileName()!))
            .Where(v => v is not null);
        return missingVariants.ToImmutableArray();
    }
}
