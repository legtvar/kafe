namespace Kafe.Data.Events;

public record GlobalPermissionsChanged(
    [Hrib] string EntityId,
    Permission GlobalPermissions
);
