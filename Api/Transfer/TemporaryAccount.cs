namespace Kafe.Api.Transfer;

public record TemporaryAccountCreationDto(
    string EmailAddress,
    string? PreferredCulture
);

public record TemporaryAccountInfoDto(
    Hrib Id,
    string EmailAddress,
    string PreferredCulture
);

public record TemporaryAccountTokenDto(
    Hrib AccountId,
    string Purpose,
    string SecurityStamp
);
