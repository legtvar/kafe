namespace Kafe.Data;

public record NotificationValidationCreated;
public record NotificationConversionFailureCreated(
    string ProjectId,
    string VideoId,
    string Reason
);
public record NotificationDramaturgyCreated(
    string ProjectId,
    bool IsAccepted,
    string AdditionalInfo
);
public record NotificationSent(
    string EmailAddress
);
