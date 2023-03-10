using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record PlaylistCreated(
    [Hrib] string PlaylistId,
    CreationMethod CreationMethod,
    [LocalizedString] ImmutableDictionary<string, string> Name,
    Visibility Visibility
);
public record PlaylistInfoChanged(
    [Hrib] string PlaylistId,
    [LocalizedString] ImmutableDictionary<string, string>? Name = null,
    [LocalizedString] ImmutableDictionary<string, string>? Description = null,
    Visibility? Visibility = null
);
public record PlaylistVideoAdded(
    [Hrib] string PlaylistId,
    [Hrib] string VideoId
);
public record PlaylistVideoRemoved(
    [Hrib] string PlaylistId,
    [Hrib] string VideoId
);
public record PlaylistVideoOrderChanged(
    [Hrib] string PlaylistId,
    [Hrib] string VideoId,
    int NewIndex
);
