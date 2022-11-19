namespace Kafe.Data.Events;

public record PlaylistCreated(
    CreationMethod CreationMethod
);
public record PlaylistInfoChanged(
    string? Name = null,
    string? Description = null,
    string? EnglishName = null,
    string? EnglishDescription = null,
    Visibility Visibility = Visibility.Unknown
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
