namespace Kafe.Data.Events;

public record NotificationCreated(
    CreationMethod CreationMethod,
    NotificationKind Kind,
    List<string> Recipients,
    string? ProjectId,
    string? VideoId,
    string? Description,
    string? EnglishDescription
);

public record NotificationSent;
