namespace Kafe.Data.Events;

public record PlaylistCreated(
    Hrib PlaylistId,
    CreationMethod CreationMethod,
    LocalizedString Name,
    Visibility Visibility
);
public record PlaylistInfoChanged(
    Hrib PlaylistId,
    LocalizedString? Name = null,
    LocalizedString? Description = null,
    Visibility? Visibility = null
);
public record PlaylistVideoAdded(
    Hrib PlaylistId,
    Hrib VideoId
);
public record PlaylistVideoRemoved(
    Hrib PlaylistId,
    Hrib VideoId
);
public record PlaylistVideoOrderChanged(
    Hrib PlaylistId,
    Hrib VideoId,
    int NewIndex
);
