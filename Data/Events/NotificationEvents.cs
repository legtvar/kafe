using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record NotificationCreated(
    [Hrib] string NotificationId,
    CreationMethod CreationMethod,
    NotificationKind Kind,
    ImmutableArray<string>? Recipients,
    [Hrib] string? ProjectId,
    [LocalizedString] ImmutableDictionary<string, string> Description
);

public record NotificationSent;
