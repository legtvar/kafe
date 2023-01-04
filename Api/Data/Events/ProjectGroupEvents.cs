namespace Kafe.Data.Events;

public record ProjectGroupCreated(
    CreationMethod CreationMethod,
    LocalizedString Name
);
public record ProjectGroupInfoChanged(
    LocalizedString? Name = null,
    LocalizedString? Description = null,
    DateTimeOffset? Deadline = null);
public record ProjectGroupOpened;
public record ProjectGroupClosed;
public record ProjectGroupValidationRulesChanged(
    ValidationRules ValidationRules
);
