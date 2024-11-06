using System;
using System.Collections.Generic;
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
    private readonly StorageService storageService;

    public VideoConversionDaemon(
        IServiceProvider serviceProvider,
        ILogger<VideoConversionDaemon> logger,
        IMediaService mediaService,
        StorageService storageService)
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
            await HandleConversion(conversion, ct);
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
                "Found unfinished conversion '{ConversionId}' of video '{VideoShardId}' to variant '{Variant}'.",
                conversion.Id,
                conversion.VideoId,
                conversion.Variant);
            return conversion;
        }

        async Task<VideoConversionInfo?> SiftThroughRange(IReadOnlyList<VideoShardInfo> range)
        {
            var ids = range.Select(v => v.Id).ToList();
            var finishedConversions = await db.Query<VideoConversionInfo>()
                .Where(c => c.IsCompleted || c.HasFailed && ids.Contains(c.VideoId))
                .Select(c => c.VideoId)
                .ToListAsync();
            var candidateIds = ids.Except(finishedConversions).Distinct().ToList();
            var candidate = range
                .Where(v => candidateIds.Contains(v.Id))
                .Select(v => (video: v, missingVariants: GetMissingVariants(v)))
                .FirstOrDefault(p => p.missingVariants.Length > 0);
            if (candidate.video is null)
            {
                return null;
            }

            logger.LogDebug("Found video '{VideoShardId}' with missing variants.", candidate.video.Id);
            var id = Hrib.Create();
            var created = new VideoConversionCreated(
                id.ToString(),
                candidate.video.Id,
                candidate.missingVariants.First());
            db.Events.KafeStartStream<VideoConversionInfo>(id, created);
            await db.SaveChangesAsync();
            return await db.Events.AggregateStreamAsync<VideoConversionInfo>(id.ToString());
        }

        VideoConversionInfo? result = null;

        var today = DateTimeOffset.UtcNow.Date;
        var todayVideos = await db.Query<VideoShardInfo>()
            .Where(v => ((string)(object)v.CreatedAt).StartsWith(today.ToString("yyyy-MM-dd")))
            .ToListAsync();
        result = await SiftThroughRange(todayVideos);
        if (result is not null)
        {
            return result;
        }

        var yearVideos = await db.Query<VideoShardInfo>()
            .Where(v => ((string)(object)v.CreatedAt).StartsWith(today.ToString("yyyy")))
            .ToListAsync();
        result = await SiftThroughRange(yearVideos);
        if (result is not null)
        {
            return result;
        }

        var allTimeVideos = await db.Query<VideoShardInfo>()
            .ToListAsync();
        result = await SiftThroughRange(allTimeVideos);
        if (result is not null)
        {
            return result;
        }

        return null;
    }

    private async Task HandleConversion(
        VideoConversionInfo conversion,
        CancellationToken ct)
    {
        using var logScope = logger.BeginScope(
                "Conversion '{ConversionId}' of '{VideoShardId}' ({Variant})",
                conversion.Id,
                conversion.VideoId,
                conversion.Variant);
        logger.LogInformation("Started.");

        try
        {
            // NB: Check for `original` first. Without it, we can just quit.
            if (!storageService.TryGetFilePath(
                ShardKind.Video,
                conversion.VideoId,
                Const.OriginalShardVariant,
                out var originalPath))
            {
                throw new InvalidOperationException(
                    $"Cannot convert video '{conversion.VideoId}' because it has no original variant.");
            }

            MediaInfo? conversionResult = null;
            // NB: Then check if the job was already done but for some reason the DB does not know it.
            //     (This is here because I accidentally dropped the DB once.)
            if (storageService.TryGetFilePath(
                ShardKind.Video,
                conversion.VideoId,
                conversion.Variant,
                out var variantPath))
            {
                conversionResult = await mediaService.GetInfo(variantPath, ct);
                if (conversionResult.IsCorrupted)
                {
                    logger.LogWarning("Found a corrupted variant file. Removing. Retrying.");
                }
                else
                {
                    logger.LogInformation("Found a file in place of the video variant. "
                        + "Assuming it was generated by other means. Skipping conversion.");
                }
            }

            if (conversionResult is null)
            {
                logger.LogInformation("Converting.");
                var videosDir = storageService.GetShardKindDirectory(ShardKind.Video, conversion.Variant);
                if (videosDir is null || !videosDir.Exists)
                {
                    throw new InvalidOperationException(
                        $"Directory for '{conversion.Variant}' videos could not be found.");
                }

                var shardDir = videosDir.CreateSubdirectory(conversion.VideoId);

                conversionResult = await mediaService.CreateVariant(
                    filePath: originalPath,
                    preset: Video.GetPresetFromFileName(conversion.Variant),
                    outputDir: shardDir.FullName,
                    overwrite: true,
                    token: ct);
                if (!string.IsNullOrEmpty(conversionResult.Error))
                {
                    throw new InvalidOperationException($"Conversion resulted in an error: {conversion.Error}.");
                }
            }

            using var scope = serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
            await db.Events.AppendExclusive(conversion.Id, new VideoConversionCompleted(conversion.Id));
            await db.Events.AppendExclusive(conversion.VideoId, new VideoShardVariantAdded(
                conversion.VideoId,
                conversion.Variant,
                conversionResult));
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Succeeded.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed with error.");
            using var scope = serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
            await db.Events.AppendExclusive(conversion.Id, new VideoConversionFailed(
                conversion.Id,
                LocalizedString.CreateInvariant(e.Message)));
            await db.SaveChangesAsync(ct);
        }
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
