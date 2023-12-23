namespace Kafe.Data.Events;

public record ExternalAccountAssociated(
    [Hrib] string AccountId,
    CreationMethod CreationMethod,
    string EmailAddress,
    string PreferredCulture,
    string IdentityProvider,
    string? Name,
    string? Uco
);
