namespace Kafe.Data;

public record PlaylistCreated;
public record PlaylistInfoChanged(
    string? Name,
    string? Description,
    string? EnglishName,
    string? EnglishDescription
);
public record PlaylistProjectAdded(
    string ProjectId
);
public record PlaylistProjectRemoved(
    string ProjectId
);
public record PlaylistProjectOrderChanged(
    string ProjectId,
    int NewIndex
);
