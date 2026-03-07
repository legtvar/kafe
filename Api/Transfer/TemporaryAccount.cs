namespace Kafe.Api.Transfer;

public record TemporaryAccountCreationDto(
    string EmailAddress,
    string? PreferredCulture
);
