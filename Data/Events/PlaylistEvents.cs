using System.Collections.Immutable;

namespace Kafe.Data.Events;

/// <summary>
/// When a playlist gets created.
/// </summary>
/// <param name="OrganizationId">
/// The Id of the group's owning organization.
/// Is nullable, since organizations were added after project groups.
/// </param>
public record PlaylistCreated(
    [Hrib] string PlaylistId,
    CreationMethod CreationMethod,
    [Hrib] string? OrganizationId,
    [LocalizedString] ImmutableDictionary<string, string> Name
);

public record PlaylistInfoChanged(
    [Hrib] string PlaylistId,
    [LocalizedString] ImmutableDictionary<string, string>? Name = null,
    [LocalizedString] ImmutableDictionary<string, string>? Description = null
);

public record PlaylistEntryAppended(
    [Hrib] string PlaylistId,
    [Hrib] string ArtifactId
);

public record PlaylistEntryRemovedFirst(
    [Hrib] string PlaylistId,
    [Hrib] string ArtifactId);

public record PlaylistEntriesSet(
    [Hrib] string PlaylistId,
    ImmutableArray<string> EntryIds);

public record PlaylistGlobalPermissionsChanged(
    [Hrib] string PlaylistId,
    Permission GlobalPermissions
);

public record PlaylistMovedToOrganization(
    [Hrib] string PlaylistId,
    [Hrib] string OrganizationId
);
