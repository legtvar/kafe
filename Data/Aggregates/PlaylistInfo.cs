using System.Collections.Immutable;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record PlaylistInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    [KafeType(typeof(ImmutableArray<Hrib>))] ImmutableArray<string> EntryIds,
    [LocalizedString] ImmutableDictionary<string, string> Name,
    [LocalizedString] ImmutableDictionary<string, string>? Description = null,
    Permission GlobalPermissions = Permission.None
) : IVisibleEntity;

public class PlaylistInfoProjection : SingleStreamProjection<PlaylistInfo>
{
    public PlaylistInfoProjection()
    {
    }

    public static PlaylistInfo Create(PlaylistCreated e)
    {
        return new PlaylistInfo(
            Id: e.PlaylistId,
            CreationMethod: e.CreationMethod,
            EntryIds: ImmutableArray.Create<string>(),
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

    public PlaylistInfo Apply(PlaylistEntryAppended e, PlaylistInfo p)
    {
        return p with
        {
            EntryIds = p.EntryIds.Add(e.ArtifactId)
        };
    }

    public PlaylistInfo Apply(PlaylistEntryRemovedFirst e, PlaylistInfo p)
    {
        return p with
        {
            EntryIds = p.EntryIds.Remove(e.ArtifactId)
        };
    }

    public PlaylistInfo Apply(PlaylistEntriesSet e, PlaylistInfo p)
    {
        return p with
        {
            EntryIds = p.EntryIds
        };
    }

    public PlaylistInfo Apply(GlobalPermissionsChanged e, PlaylistInfo a)
    {
        return a with
        {
            GlobalPermissions = e.GlobalPermissions
        };
    }
}
