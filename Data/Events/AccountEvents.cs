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
    string EntityId,
    Permission Permission
);

public record AccountPermissionUnset(
    [Hrib] string AccountId,
    string EntityId
);
