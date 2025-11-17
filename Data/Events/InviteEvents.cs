namespace Kafe.Data.Events;

public record InviteCreated(
    [Hrib]
    string InviteId,
    CreationMethod CreationMethod,
    string EmailAddress,
    string? PreferredCulture
);

public record InviteCanceled(
    [Hrib]
    string InviteId
);

public record InviteAccepted(
    [Hrib]
    string InviteId
);

public record InvitePermissionSet(
    [Hrib]
    string InviteId,
    Permission Permission,
    [Hrib]
    string EntityId
);
