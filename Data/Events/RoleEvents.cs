using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record RoleCreated(
    [Hrib] string RoleId,
    CreationMethod CreationMethod,
    [Hrib] string OrganizationId,
    [LocalizedString] ImmutableDictionary<string, string> Name
);

public record RoleInfoChanged(
    [Hrib] string RoleId,
    [LocalizedString] ImmutableDictionary<string, string>? Name,
    [LocalizedString] ImmutableDictionary<string, string>? Description
);

public record RolePermissionSet(
    [Hrib] string RoleId,
    [Hrib] string EntityId,
    Permission Permission
);

public record RolePermissionUnset(
    [Hrib] string RoleId,
    [Hrib] string EntityId
);
