namespace Kafe.Data.Events;

public record PlaylistCreated(
    CreationMethod CreationMethod,
    LocalizedString Name,
    Visibility Visibility
);
public record PlaylistInfoChanged(
    LocalizedString? Name = null,
    LocalizedString? Description = null,
    Visibility? Visibility = null
);
public record PlaylistVideoAdded(
    string VideoId
);
public record PlaylistVideoRemoved(
    string VideoId
);
public record PlaylistVideoOrderChanged(
    string VideoId,
    int NewIndex
);
