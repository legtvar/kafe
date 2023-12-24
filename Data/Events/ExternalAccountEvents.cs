namespace Kafe.Data.Events;

public record ExternalAccountAssociated(
    [Hrib] string AccountId,
    string IdentityProvider,
    string? Name,
    string? Uco
);
