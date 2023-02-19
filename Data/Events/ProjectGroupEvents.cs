using System;

namespace Kafe.Data.Events;

public record ProjectGroupCreated(
    Hrib ProjectGroupId,
    CreationMethod CreationMethod,
    LocalizedString Name
);

public record ProjectGroupInfoChanged(
    Hrib ProjectGroupId,
    LocalizedString? Name = null,
    LocalizedString? Description = null,
    DateTimeOffset? Deadline = null);

public record ProjectGroupOpened(
    Hrib ProjectGroupId
);

public record ProjectGroupClosed(
    Hrib ProjectGroupId
);

public record ProjectGroupValidationRulesChanged(
    Hrib ProjectGroupId,
    ValidationRules ValidationRules
);
