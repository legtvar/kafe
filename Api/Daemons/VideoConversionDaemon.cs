using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
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
            if (video is null)
            {
                await Task.Delay(TimeSpan.FromHours(1), ct);
            }
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
        var todayVideos = await db.Query<VideoShardInfo>()
            .Where(v => ((string)(object)v.CreatedAt).StartsWith(today.ToString("yyyy-MM-dd")))
            .ToListAsync();
        var candidateTodayVideo = todayVideos.Select(v => (video: v, missingVariants: GetMissingVariants(v)))
            .FirstOrDefault(p => p.missingVariants.Length > 0);
        if (candidateTodayVideo.video is not null)
        {
            logger.LogDebug("Found today's video '{}' with missing variants.", candidateTodayVideo.video.Id);
            var id = Hrib.Create();
            var created = new VideoConversionCreated(
                id,
                candidateTodayVideo.video.Id,
                candidateTodayVideo.missingVariants.First());
            db.Events.StartStream<VideoConversionInfo>(id, created);
            await db.SaveChangesAsync();
            return await db.Events.AggregateStreamAsync<VideoConversionInfo>(id);
        }
        
        var yearVideos = await db.Query<VideoShardInfo>()
            .Where(v => ((string)(object)v.CreatedAt).StartsWith(today.ToString("yyyy")))
            .ToListAsync();
        var candidateYearVideo = todayVideos.Select(v => (video: v, missingVariants: GetMissingVariants(v)))
            .FirstOrDefault(p => p.missingVariants.Length > 0);
        if (candidateYearVideo.video is not null)
        {
            logger.LogDebug("Found this year's video '{}' with missing variants.", candidateYearVideo.video.Id);
            var id = Hrib.Create();
            var created = new VideoConversionCreated(
                id,
                candidateYearVideo.video.Id,
                candidateYearVideo.missingVariants.First());
            db.Events.StartStream<VideoConversionInfo>(id, created);
            await db.SaveChangesAsync();
            return await db.Events.AggregateStreamAsync<VideoConversionInfo>(id);
        }
        
        var allTimeVideos = await db.Query<VideoShardInfo>()
            .ToListAsync();
        var candidateAllTimeVideo = todayVideos.Select(v => (video: v, missingVariants: GetMissingVariants(v)))
            .FirstOrDefault(p => p.missingVariants.Length > 0);
        if (candidateAllTimeVideo.video is not null)
        {
            logger.LogDebug("Found this year's video '{}' with missing variants.", candidateAllTimeVideo.video.Id);
            var id = Hrib.Create();
            var created = new VideoConversionCreated(
                id,
                candidateAllTimeVideo.video.Id,
                candidateAllTimeVideo.missingVariants.First());
            db.Events.StartStream<VideoConversionInfo>(id, created);
            await db.SaveChangesAsync();
            return await db.Events.AggregateStreamAsync<VideoConversionInfo>(id);
        }

        return null;
    }

    private static ImmutableArray<string> GetMissingVariants(VideoShardInfo video)
    {
        if (!video.Variants.TryGetValue(Const.OriginalShardVariant, out var originalVariant))
        {
            throw new ArgumentException($"VideoShard '{video.Id}' is missing the 'original' variant.");
        }
        
        var desiredVariants = Video.GetApplicablePresets(originalVariant);
        var missingVariants = desiredVariants.Select(p => p.ToFileName()!)
            .Where(v => v is not null)
            .Except(video.Variants.Keys);
        return missingVariants.ToImmutableArray();
    }
}
