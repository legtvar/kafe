using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record AuthorCreated(
    [Hrib] string AuthorId,
    CreationMethod CreationMethod,
    string Name,
    Visibility Visibility
);

public record AuthorInfoChanged(
    [Hrib] string AuthorId,
    string? Name = null,
    Visibility? Visibility = null,
    [LocalizedString] ImmutableDictionary<string, string>? Bio = null,
    string? Uco = null,
    string? Email = null,
    string? Phone = null
);
