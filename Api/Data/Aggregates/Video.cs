using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record Video(
    string Id,
    CreationMethod CreationMethod,
    LocalizedString Name,
    string? FileName,
    MediaInfo Metadata) : IEntity;

public class VideoProjections : SingleStreamAggregation<Video>
{
    public VideoProjections()
    {
    }

    public Video Create(IEvent<VideoCreated> e)
    {
        return new Video(
            Id: e.StreamKey!,
            CreationMethod: e.Data.CreationMethod,
            Name: e.Data.Name,
            FileName: e.Data.FileName,
            Metadata: e.Data.Metadata);
    }

    public Video Apply(VideoInfoChanged e, Video v)
    {
        return v with
        {
            Name = e.Name ?? v.Name,
            FileName = e.FileName ?? v.FileName,
            Metadata = e.Metadata ?? v.Metadata
        };
    }
}
