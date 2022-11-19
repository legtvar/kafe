using System.Collections.Immutable;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record Playlist(
    string Id,
    CreationMethod CreationMethod,
    ImmutableArray<string> Videos,
    string? Name = null,
    string? Description = null,
    string? EnglishName = null,
    string? EnglishDescription = null,
    Visibility Visibility = Visibility.Unknown
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
            Videos: ImmutableArray.Create<string>()
        );
    }

    public Playlist Apply(PlaylistInfoChanged e, Playlist p)
    {
        return p with
        {
            Name = e.Name,
            Description = e.Description,
            EnglishName = e.EnglishName,
            EnglishDescription = e.EnglishDescription,
            Visibility = e.Visibility
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
