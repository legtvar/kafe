namespace Kafe.Data.Events;

public record InviteCreated(
    [Hrib] string InviteId,
    CreationMethod CreationMethod,
    string EmailAddress
);

public record InviteDestroyed(
    [Hrib] string InviteId
);

public record InvitePermissionSet(
    [Hrib] string InviteId,
    [Hrib] string InviterAccountId,
    Permission Permission,
    [Hrib] string EntityId
);