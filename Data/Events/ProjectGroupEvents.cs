using System;
using System.Collections.Immutable;
using System.Security;

namespace Kafe.Data.Events;

public record ProjectGroupCreated(
    [Hrib] string ProjectGroupId,
    CreationMethod CreationMethod,
    [LocalizedString] ImmutableDictionary<string, string> Name
);

public record ProjectGroupInfoChanged(
    [Hrib] string ProjectGroupId,
    [LocalizedString] ImmutableDictionary<string, string>? Name = null,
    [LocalizedString] ImmutableDictionary<string, string>? Description = null,
    DateTimeOffset? Deadline = null,
    Permission? GlobalPermissions = null);

public record ProjectGroupOpened(
    [Hrib] string ProjectGroupId
);

public record ProjectGroupClosed(
    [Hrib] string ProjectGroupId
);

public record ProjectGroupGlobalPermissionsChanged(
    [Hrib] string ProjectGroupId,
    Permission GlobalPermissions
);
