using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Api.Options;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Data.Services;
using Kafe.Media;
using Kafe.Media.Services;
using Marten;
using Marten.Linq;
using Marten.Linq.MatchesSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kafe.Api.Daemons;

public class VideoConversionDaemon(
    IServiceProvider serviceProvider,
    ILogger<VideoConversionDaemon> logger,
    IMediaService mediaService,
    StorageService storageService,
    IOptions<VideoConversionOptions> options
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var conversion = await FindConversionToHandle(ct);
            if (conversion is null)
            {
                logger.LogInformation("Found no videos to convert. Waiting.");
                await Task.Delay(options.Value.PollWaitTime, ct);
                continue;
            }

            await HandleConversion(conversion, ct);
        }
    }

    private async Task<VideoConversionInfo?> FindConversionToHandle(CancellationToken ct = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        await using var db = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
        var unfinishedConversions = await db.Query<VideoConversionInfo>()
            .Where(i => !i.IsCompleted && !i.HasFailed)
            .ToListAsync(ct);
        if (unfinishedConversions.Count > 0)
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
            video = await FindVideoInRange(db, range, ct);
            if (video is not null)
            {
                break;
            }
        }

        if (video is null)
        {
            video = await FindVideoInRange(db, null, ct);
        }

        if (video is null)
        {
            return null;
        }

        var missingVariants = GetMissingVariants(video);
        VideoConversionInfo? newConversion = null;
        foreach (var variant in missingVariants)
        {
            var id = Hrib.Create();
            var created = new VideoConversionCreated(
                id.ToString(),
                video.Id,
                variant
            );
            db.Events.KafeStartStream<VideoConversionInfo>(id, created);
            await db.SaveChangesAsync(ct);
            newConversion ??= await db.Events.AggregateStreamAsync<VideoConversionInfo>(id.ToString(), token: ct);
        }

        return newConversion;
    }

    private async Task<VideoShardInfo?> FindVideoInRange(
        IDocumentSession db,
        TimeSpan? range,
        CancellationToken ct = default
    )
    {
        var query = db.Query<VideoShardInfo>();
        if (range is not null)
        {
            var rangeStart = DateTimeOffset.UtcNow - range.Value;
            query = (IMartenQueryable<VideoShardInfo>)query.Where(v => v.CreatedAt > rangeStart);
        }

        query = (IMartenQueryable<VideoShardInfo>)query.Where(v => v.MatchesSql(
                $"""
                 NOT EXISTS (
                 	SELECT * FROM {db.DocumentStore.Options.Schema.For<VideoConversionInfo>()} as conversion
                 	WHERE conversion.data ->> '{nameof(VideoConversionInfo.VideoId)}' = video.data ->> '{nameof(VideoShardInfo.Id)}'
                 	    AND ((conversion.data -> '{nameof(VideoConversionInfo.IsCompleted)}')::boolean
                 	        OR (conversion.data -> '{nameof(VideoConversionInfo.HasFailed)}')::boolean
                 	    )
                 )
                 """
            )
        );

        query = (IMartenQueryable<VideoShardInfo>)query.OrderByDescending(v => v.CreatedAt);
        var video = await query.FirstOrDefaultAsync(ct);
        if (video is null)
        {
            logger.LogDebug("No video requiring conversion found in the '{Range}' range.", range);
        }

        return video;
    }

    private async Task HandleConversion(
        VideoConversionInfo conversion,
        CancellationToken ct
    )
    {
        using var logScope = logger.BeginScope(
            "Conversion '{ConversionId}' of '{VideoShardId}' ({Variant})",
            conversion.Id,
            conversion.VideoId,
            conversion.Variant
        );
        logger.LogInformation("Started.");

        try
        {
            // NB: Check for `original` first. Without it, we can just quit.
            if (!storageService.TryGetFilePath(
                    ShardKind.Video,
                    conversion.VideoId,
                    Const.OriginalShardVariant,
                    out var originalPath
                ))
            {
                throw new InvalidOperationException(
                    $"Cannot convert video '{conversion.VideoId}' because it has no original variant."
                );
            }

            MediaInfo? conversionResult = null;
            // NB: Then check if the job was already done but for some reason the DB does not know it.
            //     (This is here because I accidentally dropped the DB once.)
            if (storageService.TryGetFilePath(
                    ShardKind.Video,
                    conversion.VideoId,
                    conversion.Variant,
                    out var variantPath
                ))
            {
                conversionResult = await mediaService.GetInfo(variantPath, ct);
                if (conversionResult.IsCorrupted)
                {
                    logger.LogWarning("Found a corrupted variant file. Removing. Retrying.");
                }
                else
                {
                    logger.LogInformation(
                        "Found a file in place of the video variant. "
                        + "Assuming it was generated by other means. Skipping conversion."
                    );
                }
            }

            if (conversionResult is null)
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

                if (options.Value.IsDry)
                {
                    conversionResult = await mediaService.CreateVariant(
                        filePath: originalPath,
                        preset: Video.GetPresetFromFileName(conversion.Variant),
                        outputDir: shardDir.FullName,
                        overwrite: true,
                        token: ct
                    );
                }
                else
                {
                    conversionResult = MediaInfo.Invalid with
                    {
                        Error = $"Variant not created because "
                        + $"`{nameof(VideoConversionOptions)}.{nameof(VideoConversionOptions.IsDry)}` is enabled."
                    };
                }

                if (!string.IsNullOrEmpty(conversionResult.Error))
                {
                    throw new InvalidOperationException($"Conversion resulted in an error: {conversion.Error}.");
                }
            }

            using var scope = serviceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
            await db.Events.AppendExclusive(
                conversion.Id,
                ct,
                new VideoConversionCompleted(conversion.Id)
            );
            await db.Events.AppendExclusive(
                conversion.VideoId,
                ct,
                new VideoShardVariantAdded(
                    conversion.VideoId,
                    conversion.Variant,
                    conversionResult
                )
            );
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Succeeded.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed with error.");
            using var scope = serviceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
            await db.Events.AppendExclusive(
                conversion.Id,
                ct,
                new VideoConversionFailed(
                    conversion.Id,
                    LocalizedString.CreateInvariant(e.Message)
                )
            );
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
        var missingVariants = desiredVariants.Select(p => p.ToFileName())
            .Where(v => v is not null)
            .Except(video.Variants.Keys);
        return [..missingVariants!];
    }
}
