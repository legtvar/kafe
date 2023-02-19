using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record NotificationCreated(
    Hrib NotificationId,
    CreationMethod CreationMethod,
    NotificationKind Kind,
    ImmutableArray<string>? Recipients,
    Hrib? ProjectId,
    Hrib? VideoId,
    LocalizedString Description
);

public record NotificationSent;
