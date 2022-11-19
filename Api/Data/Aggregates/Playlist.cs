using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record Playlist(
    string Id,
    CreationMethod CreationMethod,
    string? Name = null,
    string? Description = null,
    string? EnglishName = null,
    string? EnglishDescription = null,
    Visibility Visibility = Visibility.Unknown,
    List<string>? Videos = null
);

public class PlaylistProjection : SingleStreamAggregation<Playlist>
{
    public PlaylistProjection()
    {
    }

    public Playlist Create(IEvent<PlaylistCreated> e)
    {
        return new Playlist(
            Id: e.StreamKey!,
            CreationMethod: e.Data.CreationMethod,
            Videos: new List<string>()
        );
    }

    public Playlist Apply(PlaylistInfoChanged e, Playlist p)
    {
        return p with
        {
            Name = p.Name,
            Description = p.Description,
            EnglishName = p.EnglishName,
            EnglishDescription = p.EnglishDescription,
            Visibility = p.Visibility,
            Videos = new List<string>()
        };
    }

    public Playlist Apply(PlaylistVideoAdded e, Playlist p)
    {
        p.Videos!.Add(e.VideoId);
        return p;
    }
    
    public Playlist Apply(PlaylistVideoRemoved e, Playlist p)
    {
        p.Videos!.RemoveAll(v => v == e.VideoId);
        return p;
    }
    
    public Playlist Apply(PlaylistVideoOrderChanged e, Playlist p)
    {
        p.Videos!.RemoveAll(v => v == e.VideoId);
        p.Videos!.Insert(e.NewIndex, e.VideoId);
        return p;
    }
}
