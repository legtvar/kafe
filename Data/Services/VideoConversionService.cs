using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Data.Options;
using Kafe.Media;
using Kafe.Media.Services;
using Marten;
using Marten.Linq;
using Marten.Linq.MatchesSql;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kafe.Data.Services;

public class VideoConversionService(
    IDocumentSession db,
    ILogger<VideoConversionService> logger,
    IOptions<VideoConversionOptions> options,
    ShardService shardService,
    IMediaService mediaService,
    StorageService storageService
)
{
    public async Task<VideoConversionInfo?> Load(
        Hrib id,
        CancellationToken ct = default
    )
    {
        return (await db.KafeLoadAsync<VideoConversionInfo>(id, token: ct)).GetValueOrDefault();
    }

    public async Task<ImmutableArray<VideoConversionInfo>> LoadMany(
        IEnumerable<Hrib> ids,
        CancellationToken ct = default
    )
    {
        return (await db.KafeLoadManyAsync<VideoConversionInfo>([..ids], ct)).Unwrap();
    }

    public async Task<Err<VideoConversionInfo>> Upsert(VideoConversionInfo conversion, CancellationToken ct = default)
    {
        var id = Hrib.EnsureValid(conversion.Id, shouldReplaceEmpty: true);
        if (id.HasErrors)
        {
            return id.Errors;
        }

        var videoId = Hrib.EnsureValid(conversion.VideoId, shouldReplaceEmpty: false);
        if (videoId.HasErrors)
        {
            return videoId.Errors;
        }

        var shard = await shardService.Load((Hrib)videoId, ct);
        if (shard is null)
        {
            return Error.NotFound("The referenced video");
        }

        if (shard.Kind != ShardKind.Video)
        {
            return Error.InvalidValue("The video HRIB points to a non-video shard.");
        }

        var stream = await db.Events.FetchForWriting<VideoConversionInfo>(id.Value.ToString(), cancellation: ct);
        if (stream.Aggregate is null)
        {
            var created = new VideoConversionCreated(
                ConversionId: id.Value.ToString(),
                VideoId: videoId.Value.ToString(),
                Variant: conversion.Variant
            );
            db.Events.KafeStartStream<VideoConversionInfo>((Hrib)id, created);
            await db.SaveChangesAsync(ct);
            stream = await db.Events.FetchForWriting<VideoConversionInfo>(id.Value.ToString(), cancellation: ct);
            if (stream.Aggregate is null)
            {
                throw new InvalidOperationException("Failed to create a new video conversion.");
            }
        }

        if (conversion is { IsCompleted: true, HasFailed: true })
        {
            return Error.InvalidValue("The video conversion cannot succeed and fail at the same time.");
        }

        if (conversion.IsCompleted && !stream.Aggregate.IsCompleted)
        {
            if (stream.Aggregate.HasFailed)
            {
                return Error.AlreadyFailed("The video conversion");
            }

            stream.AppendOne(
                new VideoConversionCompleted(id.Value.ToString())
            );
        }

        if (conversion.HasFailed
            && stream.Aggregate.HasFailed
            && conversion.Error != (LocalizedString?)stream.Aggregate.Error
           )
        {
            return Error.AlreadyFailed("The video conversion");
        }

        if ((conversion.HasFailed || !LocalizedString.IsNullOrEmpty(conversion.Error))
            && !stream.Aggregate.HasFailed
           )
        {
            if (stream.Aggregate.IsCompleted)
            {
                return Error.AlreadyCompleted("The video conversion");
            }

            stream.AppendOne(new VideoConversionFailed(id.Value.ToString(), conversion.Error));
        }

        await db.SaveChangesAsync(ct);
        return await db.Events.RequireLatest<VideoConversionInfo>((Hrib)id, ct);
    }

    public record VideoConversionFilter(
        bool? IsCompleted = false,
        bool? HasFailed = false,
        bool ShouldFetchLatestOnly = true
    );

    public async Task<ImmutableArray<VideoConversionInfo>> List(
        VideoConversionFilter? filter = null,
        CancellationToken ct = default
    )
    {
        filter ??= new VideoConversionFilter();

        if (!filter.ShouldFetchLatestOnly)
        {
            var query = db.Query<VideoConversionInfo>();
            if (filter.IsCompleted is not null)
            {
                var isCompleted = filter.IsCompleted.Value;
                query = (IMartenQueryable<VideoConversionInfo>)query.Where(v => v.IsCompleted == isCompleted);
            }

            if (filter.HasFailed is not null)
            {
                var hasFailed = filter.HasFailed.Value;
                query = (IMartenQueryable<VideoConversionInfo>)query.Where(v => v.HasFailed == hasFailed);
            }

            return [..await query.ToListAsync(ct)];
        }

        var sb = new StringBuilder();
        sb.Append(
            $"""
             SELECT a.id, a.data, a.mt_version
             FROM {db.DocumentStore.Options.Schema.For<VideoConversionInfo>()} AS a
                 LEFT OUTER JOIN {db.DocumentStore.Options.Schema.For<VideoConversionInfo>()} AS b
             	    ON a.data ->> '{nameof(VideoConversionInfo.VideoId)}' = b.data ->> '{nameof(VideoConversionInfo.VideoId)}'
             		    AND a.data ->> '{nameof(VideoConversionInfo.Variant)}' = b.data ->> '{nameof(VideoConversionInfo.Variant)}'
             		    AND (a.data ->> '{nameof(VideoConversionInfo.CreatedAt)}')::timestamptz < (b.data ->> '{nameof(VideoConversionInfo.CreatedAt)}')::timestamptz
                 WHERE b.id IS NULL
             """
        );
        if (filter.IsCompleted is not null)
        {
            var isCompleted = filter.IsCompleted.Value;
            sb.Append($" AND ((a.data ->> '{nameof(VideoConversionInfo.IsCompleted)}')::boolean = {isCompleted})");
        }

        if (filter.HasFailed is not null)
        {
            var hasFailed = filter.HasFailed.Value;
            sb.Append($" AND ((a.data ->> '{nameof(VideoConversionInfo.HasFailed)}')::boolean = {hasFailed})");
        }

        return [..await db.AdvancedSql.QueryAsync<VideoConversionInfo>(sb.ToString(), ct)];
    }

    public record VideoShardFilter(
        TimeSpan? Range = null,
        bool? HasOriginalVariant = true,
        bool? IsCorrupted = false,
        bool? HasAnyCompletedOrFailedConversions = false
    );

    public IMartenQueryable<VideoShardInfo> QueryVideoShards(VideoShardFilter? filter = null)
    {
        filter ??= new VideoShardFilter();

        var query = db.Query<VideoShardInfo>();
        if (filter.Range is not null)
        {
            var rangeStart = DateTimeOffset.UtcNow - filter.Range.Value;
            query = (IMartenQueryable<VideoShardInfo>)query.Where(v => v.CreatedAt > rangeStart);
        }

        if (filter.HasOriginalVariant is not null)
        {
            var hasOriginal = filter.HasOriginalVariant.Value;
            query = (IMartenQueryable<VideoShardInfo>)query.Where(v => v.MatchesSql(
                    $"d.data -> '{nameof(VideoShardInfo.Variants)}' "
                    + $"-> '{Const.OriginalShardVariant}' IS {(hasOriginal ? "NOT" : "")} NULL"
                )
            );
        }

        if (filter.IsCorrupted is not null)
        {
            var isCorrupted = filter.IsCorrupted.Value;
            query = (IMartenQueryable<VideoShardInfo>)query.Where(v => v.MatchesSql(
                    $"(d.data -> '{nameof(VideoShardInfo.Variants)}' -> '{Const.OriginalShardVariant}' "
                    + $"->> '{nameof(MediaInfo.IsCorrupted)}')::boolean = ?",
                    isCorrupted
                )
            );
        }

        if (filter.HasAnyCompletedOrFailedConversions is not null)
        {
            var hasCompletedOrFailedConversions = filter.HasAnyCompletedOrFailedConversions.Value;
            query = (IMartenQueryable<VideoShardInfo>)query.Where(v => v.MatchesSql(
                    $"""
                     {(hasCompletedOrFailedConversions ? "" : "NOT")} EXISTS (
                         SELECT a.*  FROM {db.DocumentStore.Options.Schema.For<VideoConversionInfo>(true)} AS a
                         LEFT OUTER JOIN {db.DocumentStore.Options.Schema.For<VideoConversionInfo>(true)} AS b
                     	    ON a.data ->> '{nameof(VideoConversionInfo.VideoId)}' = b.data ->> '{nameof(VideoConversionInfo.VideoId)}'
                     		    AND a.data ->> '{nameof(VideoConversionInfo.Variant)}' = b.data ->> '{nameof(VideoConversionInfo.Variant)}'
                     		    AND (a.data ->> '{nameof(VideoConversionInfo.CreatedAt)}')::timestamptz < (b.data ->> '{nameof(VideoConversionInfo.CreatedAt)}')::timestamptz
                         WHERE b.id is null AND a.data ->> '{nameof(VideoConversionInfo.VideoId)}' = d.id AND (
                            (a.data ->> '{nameof(VideoConversionInfo.IsCompleted)}')::boolean
                            OR (a.data ->> '{nameof(VideoConversionInfo.HasFailed)}')::boolean
                         )
                     )
                     """
                )
            );
        }

        query = (IMartenQueryable<VideoShardInfo>)query.OrderByDescending(v => v.CreatedAt);
        return query;
    }

    public async Task ConvertVideoPersist(VideoConversionInfo conversion, CancellationToken ct)
    {
        using var logScope = logger.BeginScope(
            "Conversion '{ConversionId}' of '{VideoShardId}' ({Variant})",
            conversion.Id,
            conversion.VideoId,
            conversion.Variant
        );
        logger.LogInformation("Started.");

        var mediaInfo = MediaInfo.Invalid;
        try
        {
            mediaInfo = await ConvertVideo(conversion, ct);
        }
        catch (Exception e)
        {
            mediaInfo = mediaInfo with { IsCorrupted = true, Error = e.Message };
        }

        if (mediaInfo.IsCorrupted || mediaInfo.Error is not null)
        {
            logger.LogError("Failed with error.\n{Error}", mediaInfo.Error);
            conversion = conversion with
            {
                HasFailed = true,
                Error = LocalizedString.CreateInvariant(
                    mediaInfo.Error ?? "An unspecified but definitely horrific thing happened."
                )
            };
        }
        else
        {
            conversion = conversion with
            {
                IsCompleted = true
            };
        }

        var err = await Upsert(conversion, ct);
        if (err.HasErrors)
        {
            logger.LogError(err, "Could not persist the video conversion result in the database.");
            return;
        }

        if (conversion.IsCompleted)
        {
            await db.Events.AppendExclusive(
                conversion.VideoId,
                ct,
                new VideoShardVariantAdded(
                    conversion.VideoId,
                    conversion.Variant,
                    mediaInfo
                )
            );
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Succeeded.");
        }
    }

    public async Task<MediaInfo> ConvertVideo(VideoConversionInfo conversion, CancellationToken ct = default)
    {
        var result = MediaInfo.Invalid;
        // Check for `original` first. Without it, we can just quit.
        if (!storageService.TryGetFilePath(
                ShardKind.Video,
                conversion.VideoId,
                Const.OriginalShardVariant,
                out var originalPath
            ))
        {
            result = result with { IsCorrupted = true, Error = "No original variant found." };
            return result;
        }

        // Then check if the job was already done but for some reason the DB does not know it.
        // (This is here because I accidentally dropped the DB once.)
        if (storageService.TryGetFilePath(
                ShardKind.Video,
                conversion.VideoId,
                conversion.Variant,
                out var variantPath
            ))
        {
            result = await mediaService.GetInfo(variantPath, ct);
            if (result.IsCorrupted)
            {
                logger.LogWarning("Found a corrupted variant file. Removing. Retrying.");
            }
            else
            {
                logger.LogInformation(
                    "Found a file in place of the video variant. "
                    + "Assuming it was generated by other means. Skipping conversion."
                );
                return result;
            }
        }

        // IsCorrupted is true for both corrupted and non-existent media files.
        if (result.IsCorrupted)
        {
            logger.LogInformation("Converting.");
            var videosDir = storageService.GetShardKindDirectory(ShardKind.Video, conversion.Variant);
            if (videosDir is null || !videosDir.Exists)
            {
                throw new InvalidOperationException(
                    $"Directory for '{conversion.Variant}' videos could not be found."
                );
            }

            var shardDir = videosDir.CreateSubdirectory(conversion.VideoId);

            result = await mediaService.CreateVariant(
                filePath: originalPath,
                preset: Video.GetPresetFromFileName(conversion.Variant),
                outputDir: shardDir.FullName,
                overwrite: true,
                isDryRun: options.Value.IsDry,
                token: ct
            );

            if (options.Value.IsDry)
            {
                result = result with
                {
                    IsCorrupted = true,
                    Error = "No variant has been created because dry mode is on."
                    + (string.IsNullOrEmpty(result.Error) ? "" : "\n\n" + result.Error)
                };
            }
        }

        return result;
    }

    public async Task<VideoConversionInfo?> FindConversionToHandle(CancellationToken ct = default)
    {
        var unfinishedConversions = await List(
            new VideoConversionFilter(
                IsCompleted: false,
                HasFailed: false
            ),
            ct
        );
        if (unfinishedConversions.Length > 0)
        {
            var conversion = unfinishedConversions
                .Select(c => (conversion: c, preset: Video.GetPresetFromFileName(c.Variant)))
                .Where(p => p.preset != VideoQualityPreset.Invalid && p.preset != VideoQualityPreset.Original)
                .OrderBy(p => p.preset)
                .FirstOrDefault()
                .conversion;

            if (conversion is not null)
            {
                logger.LogDebug(
                    "Found unfinished conversion '{ConversionId}' of video '{VideoShardId}' to variant '{Variant}'.",
                    conversion.Id,
                    conversion.VideoId,
                    conversion.Variant
                );
                return conversion;
            }
        }

        var sortedRanges = options.Value.FilterRanges.Order().ToImmutableArray();
        VideoShardInfo? video = null;
        foreach (var range in sortedRanges)
        {
            video = await FindVideoInRange(range, ct);
            if (video is not null)
            {
                break;
            }
        }

        // NB: use an "infinite" range as a fallback so not even ancient videos are omitted
        video ??= await FindVideoInRange(null, ct);

        if (video is null)
        {
            return null;
        }

        var missingVariants = GetMissingVariants(video);
        VideoConversionInfo? newConversion = null;
        foreach (var variant in missingVariants)
        {
            var err = await Upsert(
                new VideoConversionInfo(
                    Id: Hrib.EmptyValue,
                    VideoId: video.Id,
                    Variant: variant
                ),
                ct
            );
            if (err.HasErrors)
            {
                logger.LogError(err, "One or more errors occurred while creating a video conversion.");
                continue;
            }

            newConversion ??= err.Value;
        }

        return newConversion;
    }

    public async Task<Err<bool>> RetryConversions(
        IEnumerable<Hrib>? ids = null,
        CancellationToken ct = default
    )
    {
        ImmutableArray<VideoConversionInfo> conversions;
        if (ids is not null)
        {
            conversions = await LoadMany(ids, ct);
        }
        else
        {
            conversions = await List(
                new VideoConversionFilter(
                    IsCompleted: null,
                    HasFailed: true
                ),
                ct
            );
        }

        logger.LogInformation("Retrying {ConversionCount} video conversions.", conversions.Length);
        foreach (var conversion in conversions)
        {
            var err = await Upsert(
                new VideoConversionInfo(
                    Id: Hrib.EmptyValue,
                    VideoId: conversion.VideoId,
                    Variant: conversion.Variant
                ),
                ct
            );
            if (err.HasErrors)
            {
                return err.Errors;
            }
        }

        return true;
    }

    public async Task<Err<bool>> RetryOriginalAnalysis(
        IEnumerable<Hrib>? shardIds = null,
        CancellationToken ct = default
    )
    {
        ImmutableArray<VideoShardInfo> videos;
        if (shardIds is not null)
        {
            videos = await shardService.LoadMany<VideoShardInfo>(shardIds, ct);
        }
        else
        {
            videos =
            [
                ..await QueryVideoShards(
                        new VideoShardFilter(
                            HasOriginalVariant: true,
                            IsCorrupted: true,
                            HasAnyCompletedOrFailedConversions: null
                        )
                    )
                    .ToListAsync(ct)
            ];
        }

        logger.LogInformation("Retrying {AnalysisCount} media analysis of original variants.", videos.Length);

        var result = new Err<bool>();

        foreach (var video in videos)
        {
            if (!storageService.TryGetFilePath(
                    ShardKind.Video,
                    video.Id,
                    Const.OriginalShardVariant,
                    out var shardFilePath
                ))
            {
                logger.LogError("Cound not find original variant of video shard {VideoShardId}.", video.Id);
                result = result.WithErrors(
                    Error.NotFound($"Original variant of shard '{video.Id}' could not be found.")
                );
                continue;
            }

            var mediaInfo = await mediaService.GetInfo(shardFilePath, ct);
            if (mediaInfo.IsCorrupted)
            {
                var error = Error.AnalysisFailed(video.Id, mediaInfo.Error);
                result = result.WithErrors(error);
                logger.LogError(error, "Retry of media analysis of video shard {VideoShardId} failed.");
                continue;
            }

            db.Events.Append(
                video.Id,
                new VideoShardVariantAdded(
                    ShardId: video.Id,
                    Name: Const.OriginalShardVariant,
                    Info: mediaInfo
                )
            );
            await db.SaveChangesAsync(ct);
        }

        return result;
    }

    private async Task<VideoShardInfo?> FindVideoInRange(
        TimeSpan? range,
        CancellationToken ct = default
    )
    {
        var query = QueryVideoShards(
            new VideoShardFilter(
                Range: range,
                HasOriginalVariant: true,
                IsCorrupted: false,
                HasAnyCompletedOrFailedConversions: false
            )
        );
        var video = await query.FirstOrDefaultAsync(ct);
        if (video is null)
        {
            logger.LogDebug("No video requiring conversion found in the '{Range}' range.", range);
        }

        return video;
    }

    private static ImmutableArray<string> GetMissingVariants(VideoShardInfo video)
    {
        if (!video.Variants.TryGetValue(Const.OriginalShardVariant, out var originalVariant))
        {
            throw new ArgumentException($"VideoShard '{video.Id}' is missing the 'original' variant.");
        }

        var desiredVariants = Video.GetApplicablePresets(originalVariant);
        var missingVariants = desiredVariants.Select(p => p.ToFileName())
            .Where(v => v is not null)
            .Except(video.Variants.Keys);
        return [..missingVariants!];
    }
}
