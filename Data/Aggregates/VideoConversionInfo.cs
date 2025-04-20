using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using System.Collections.Immutable;

namespace Kafe.Data.Aggregates;

public record VideoConversionInfo(
    [Hrib] string Id,
    string VideoId,
    string Variant,
    bool IsCompleted = false,
    bool HasFailed = false,
    [LocalizedString] ImmutableDictionary<string, string>? Error = null
) : IEntity
{
    Hrib IEntity.Id => Id;
}

public class VideoConversionInfoProjection : SingleStreamProjection<VideoConversionInfo>
{
    public VideoConversionInfoProjection()
    {
    }

    public static VideoConversionInfo Create(VideoConversionCreated e)
    {
        return new VideoConversionInfo(
            Id: e.ConversionId,
            VideoId: e.VideoId,
            Variant: e.Variant);
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
