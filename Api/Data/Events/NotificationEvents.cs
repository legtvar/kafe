using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record NotificationCreated(
    CreationMethod CreationMethod,
    NotificationKind Kind,
    ImmutableArray<string>? Recipients,
    string? ProjectId,
    string? VideoId,
    LocalizedString Description
);

public record NotificationSent;
