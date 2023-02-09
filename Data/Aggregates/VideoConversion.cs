using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record VideoConversion(
    string Id,
    string VideoId,
    bool IsCompleted = false,
    bool HasFailed = false,
    LocalizedString? Error = null
);

public class VideoConversionProjection : SingleStreamAggregation<VideoConversion>
{
    public VideoConversionProjection()
    {
    }

    public VideoConversion Create(IEvent<VideoConversionCreated> e)
    {
        return new VideoConversion(
            Id: e.StreamKey!,
            VideoId: e.Data.VideoId);
    }

    public VideoConversion Apply(VideoConversionCompleted e, VideoConversion c)
    {
        return c with { IsCompleted = true };
    }

    public VideoConversion Apply(VideoConversionFailed e, VideoConversion c)
    {
        return c with { HasFailed = true, Error = e.Reason };
    }
}
