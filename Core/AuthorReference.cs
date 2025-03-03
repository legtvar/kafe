namespace Kafe.Core;

public record AuthorReference(
    Hrib? AuthorId,
    string? Name,
    string[] Roles
);

