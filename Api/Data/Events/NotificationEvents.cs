using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record NotificationCreated(
    CreationMethod CreationMethod,
    NotificationKind Kind,
    ImmutableArray<string>? Recipients,
    string? ProjectId,
    string? VideoId,
    string? Description,
    string? EnglishDescription
);

public record NotificationSent;
