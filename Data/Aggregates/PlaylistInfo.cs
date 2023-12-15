using System.Collections.Immutable;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record PlaylistInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    [KafeType(typeof(ImmutableArray<Hrib>))] ImmutableArray<string> VideoIds,
    [LocalizedString] ImmutableDictionary<string, string> Name,
    [LocalizedString] ImmutableDictionary<string, string>? Description = null,
    Permission GlobalPermissions = Permission.None 
) : IVisibleEntity;

public class PlaylistInfoProjection : SingleStreamProjection<PlaylistInfo>
{
    public PlaylistInfoProjection()
    {
    }

    public PlaylistInfo Create(PlaylistCreated e)
    {
        return new PlaylistInfo(
            Id: e.PlaylistId,
            CreationMethod: e.CreationMethod,
            VideoIds: ImmutableArray.Create<string>(),
            Name: e.Name
        );
    }

    public PlaylistInfo Apply(PlaylistInfoChanged e, PlaylistInfo p)
    {
        return p with
        {
            Name = e.Name ?? p.Name,
            Description = e.Description ?? p.Name,
            GlobalPermissions = e.GlobalPermissions ?? p.GlobalPermissions
        };
    }

    public PlaylistInfo Apply(PlaylistVideoAdded e, PlaylistInfo p)
    {
        p.VideoIds!.Add(e.VideoId);
        return p;
    }
    
    public PlaylistInfo Apply(PlaylistVideoRemoved e, PlaylistInfo p)
    {
        p.VideoIds!.RemoveAll(v => v == e.VideoId);
        return p;
    }
    
    public PlaylistInfo Apply(PlaylistVideoOrderChanged e, PlaylistInfo p)
    {
        p.VideoIds!.RemoveAll(v => v == e.VideoId);
        p.VideoIds!.Insert(e.NewIndex, e.VideoId);
        return p;
    }
}
