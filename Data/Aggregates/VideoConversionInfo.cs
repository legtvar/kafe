using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record VideoConversionInfo(
    string Id,
    string VideoId,
    bool IsCompleted = false,
    bool HasFailed = false,
    LocalizedString? Error = null
);

public class VideoConversionInfoProjection : SingleStreamAggregation<VideoConversionInfo>
{
    public VideoConversionInfoProjection()
    {
    }

    public VideoConversionInfo Create(VideoConversionCreated e)
    {
        return new VideoConversionInfo(
            Id: e.ConversionId,
            VideoId: e.VideoId);
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
