using System;

namespace Kafe.Api.Transfer;
public record OrganizationDetailDto(
    Hrib Id,
    LocalizedString Name,
    DateTimeOffset CreatedOn
);

public record OrganizationListDto(
    Hrib Id,
    LocalizedString Name
);

public record OrganizationCreationDto(
    LocalizedString Name
);
