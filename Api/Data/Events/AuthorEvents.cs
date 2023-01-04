namespace Kafe.Data.Events;

public record AuthorCreated(
    CreationMethod CreationMethod,
    string Name
);
public record AuthorInfoChanged(
    string? Name = null,
    string? Uco = null,
    string? Email = null,
    string? Phone = null
);
