using System;
using Kafe.Data.Events;
using Marten.Events.Aggregation;
using System.Collections.Immutable;
using JasperFx.Events;

namespace Kafe.Data.Aggregates;

/// <summary>
/// Information about a video conversion.
/// </summary>
/// <param name="VideoId">Id of the <see cref="VideoShardInfo"/> being converted.</param>
/// <param name="Variant">Name of the video variant being created.</param>
/// <param name="IsCompleted">Has it already been successfully completed?</param>
/// <param name="HasFailed">Has it already failed?</param>
/// <param name="Error">The reason for failure.</param>
public record VideoConversionInfo(
    [Hrib] string Id,
    string VideoId,
    string Variant,
    DateTimeOffset CreatedAt = default,
    bool IsCompleted = false,
    bool HasFailed = false,
    [LocalizedString] ImmutableDictionary<string, string>? Error = null
) : IEntity;

public class VideoConversionInfoProjection : SingleStreamProjection<VideoConversionInfo, string>
{
    public VideoConversionInfoProjection()
    {
    }

    public static VideoConversionInfo Create(IEvent<VideoConversionCreated> e)
    {
        return new VideoConversionInfo(
            Id: e.Data.ConversionId,
            VideoId: e.Data.VideoId,
            Variant: e.Data.Variant,
            CreatedAt: e.Timestamp
        );
    }

    public VideoConversionInfo Apply(VideoConversionCompleted e, VideoConversionInfo c)
    {
        return c with { IsCompleted = true };
    }

    public VideoConversionInfo Apply(VideoConversionFailed e, VideoConversionInfo c)
    {
        return c with { HasFailed = true, Error = e.Reason };
    }
}
