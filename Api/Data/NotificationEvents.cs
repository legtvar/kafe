namespace Kafe.Data;

public record NotificationValidationCreated(
    CreationMethod CreationMethod,
    List<string> Recipients,
    bool IsSuccessful,
    string ProjectId,
    string? Description,
    string? DescriptionEnglish
);
public record NotificationConversionCreated(
    CreationMethod Method,
    string ProjectId,
    string VideoId,
    string? Description
);
public record NotificationDramaturgyCreated(
    string ProjectId,
    bool IsAccepted,
    string? Description,
    string? DescriptionEnglish
);
public record NotificationError(
    string Reason
);
public record NotificationSent;
