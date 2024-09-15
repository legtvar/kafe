namespace Kafe.Data.Events;

public record AccountCreated(
    [Hrib] string AccountId,
    CreationMethod CreationMethod,
    string EmailAddress,
    string PreferredCulture
);

public record AccountInfoChanged(
    [Hrib] string AccountId,
    string? PreferredCulture,
    string? Name,
    string? Uco,
    string? Phone
);

public record AccountPermissionSet(
    [Hrib] string AccountId,
    [Hrib] string EntityId,
    Permission Permission
);

public record AccountRoleSet(
    [Hrib] string AccountId,
    [Hrib] string RoleId
);

public record AccountRoleUnset(
    [Hrib] string AccountId,
    [Hrib] string RoleId
);
