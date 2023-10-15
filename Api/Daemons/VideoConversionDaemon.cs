using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Data.Services;
using Kafe.Media;
using Kafe.Media.Services;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kafe.Api.Daemons;

public class VideoConversionDaemon : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<VideoConversionDaemon> logger;
    private readonly IMediaService mediaService;
    private readonly IStorageService storageService;

    public VideoConversionDaemon(
        IServiceProvider serviceProvider,
        ILogger<VideoConversionDaemon> logger,
        IMediaService mediaService,
        IStorageService storageService)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
        this.mediaService = mediaService;
        this.storageService = storageService;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var conversion = await FindVideoToConvert();
            if (conversion is null)
            {
                logger.LogInformation("Found no videos to convert. Waiting.");
                await Task.Delay(TimeSpan.FromHours(1), ct);
                continue;
            }
            logger.LogInformation("Converting video '{}' ({}).", conversion.VideoId, conversion.Variant);

            try
            {
                if (!storageService.TryGetFilePath(
                    ShardKind.Video,
                    conversion.VideoId,
                    Const.OriginalShardVariant,
                    out var originalPath))
                {
                    throw new InvalidOperationException(
                        $"Cannot convert video '${conversion.VideoId}' because it has no original variant.");
                }

                var videosDir = storageService.GetShardKindDirectory(ShardKind.Video, conversion.Variant);
                if (videosDir is null || !videosDir.Exists)
                {
                    throw new InvalidOperationException($"The directory for the '{conversion.Variant}' variant of video " +
                        "'{video.VideoId}' could not be found.");
                }

                var shardDir = videosDir.CreateSubdirectory(conversion.VideoId);

                var conversionResult = await mediaService.CreateVariant(
                    filePath: originalPath,
                    preset: Video.GetPresetFromFileName(conversion.Variant),
                    outputDir: shardDir.FullName,
                    overwrite: true,
                    token: ct);
                if (!string.IsNullOrEmpty(conversionResult.Error))
                {
                    throw new InvalidOperationException($"Conversion resulted in an error: {conversion.Error}.");
                }

                using var scope = serviceProvider.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
                await db.Events.AppendExclusive(conversion.Id, new VideoConversionCompleted(conversion.Id));
                await db.Events.AppendExclusive(conversion.VideoId, new VideoShardVariantAdded(
                    conversion.VideoId,
                    conversion.Variant,
                    conversionResult));
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Conversion '{}' ({}) succeeded.", conversion.VideoId, conversion.Variant);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Conversion '{}' ({}) failed.", conversion.VideoId, conversion.Variant);
                using var scope = serviceProvider.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
                await db.Events.AppendExclusive(conversion.Id, new VideoConversionFailed(
                    conversion.Id,
                    LocalizedString.CreateInvariant(e.Message)));
                await db.SaveChangesAsync(ct);
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
        var candidateYearVideo = yearVideos.Select(v => (video: v, missingVariants: GetMissingVariants(v)))
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
        var candidateAllTimeVideo = allTimeVideos.Select(v => (video: v, missingVariants: GetMissingVariants(v)))
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
