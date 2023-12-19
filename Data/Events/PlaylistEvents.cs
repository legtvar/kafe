using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record PlaylistCreated(
    [Hrib] string PlaylistId,
    CreationMethod CreationMethod,
    [LocalizedString] ImmutableDictionary<string, string> Name
);
public record PlaylistInfoChanged(
    [Hrib] string PlaylistId,
    [LocalizedString] ImmutableDictionary<string, string>? Name = null,
    [LocalizedString] ImmutableDictionary<string, string>? Description = null,
    Permission? GlobalPermissions = null
);

public record PlaylistEntryAppended(
    [Hrib] string PlaylistId,
    [Hrib] string ArtifactId
);

public record PlaylistEntryRemovedFirst(
    [Hrib] string PlaylistId,
    [Hrib] string ArtifactId);
    
public record PlaylistEntryEntriesSet(
    [Hrib] string PlaylistId,
    ImmutableArray<string> EntryIds);
