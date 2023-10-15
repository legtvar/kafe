namespace Kafe.Data.Events;

public record AccountPermissionSet(
    [Hrib] string AccountId,
    string EntityId,
    Permission Permission
);

public record AccountPermissionUnset(
    [Hrib] string AccountId,
    string EntityId
);
