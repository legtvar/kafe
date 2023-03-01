namespace Kafe.Api.Transfer;

public record TemporaryAccountCreationDto(
    string EmailAddress
);

public record TemporaryAccountInfoDto(
    Hrib Id,
    string EmailAddress
);
