namespace Kafe.Data.Events;

public record AuthorCreated(
    Hrib AuthorId,
    CreationMethod CreationMethod,
    string Name
);

public record AuthorInfoChanged(
    Hrib AuthorId,
    string? Name = null,
    LocalizedString? Bio = null,
    string? Uco = null,
    string? Email = null,
    string? Phone = null
);
