namespace Kafe.Data.Events;

public record ProjectGroupCreated(
    CreationMethod CreationMethod
);
public record ProjectGroupInfoChanged(
    string? Name = null,
    string? Description = null,
    string? EnglishName = null,
    string? EnglishDescription = null,
    DateTimeOffset Deadline = default);
public record ProjectGroupOpened;
public record ProjectGroupClosed;
public record ProjectGroupValidationRulesChanged(
    ValidationRules ValidationRules
);
