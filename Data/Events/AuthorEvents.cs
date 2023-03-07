namespace Kafe.Data.Events;

public record AuthorCreated(
    Hrib AuthorId,
    CreationMethod CreationMethod,
    string Name,
    Visibility Visibility
);

public record AuthorInfoChanged(
    Hrib AuthorId,
    string? Name = null,
    Visibility? Visibility = null,
    LocalizedString? Bio = null,
    string? Uco = null,
    string? Email = null,
    string? Phone = null
);
