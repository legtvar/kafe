using System;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using Microsoft.Extensions.Logging;

namespace Kafe.Data.Services;

public class VideoConversionService(
    IDocumentSession db,
    ILogger<VideoConversionService> logger,
    ShardService shardService
)
{
    public async Task<VideoConversionInfo?> Load(
        Hrib id,
        CancellationToken ct = default
    )
    {
        return (await db.KafeLoadAsync<VideoConversionInfo>(id, token: ct)).GetValueOrDefault();
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

        var videoShard = await shardService.Load((Hrib)videoId, ct);
        if (videoShard is null)
        {
            return Error.NotFound("The referenced video");
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
}
