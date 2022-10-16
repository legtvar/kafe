namespace Kafe.Data;

public record AuthorCreated(
    CreationMethod CreationMethod
);
public record AuthorInfoChanged(
    string? Name = null,
    string? Uco = null,
    string? Email = null,
    string? Phone = null
);
