namespace Kafe;

public record RequirementMessage(
    string Id,
    LocalizedString Message,
    RequirementMessageSeverity Severity
);
