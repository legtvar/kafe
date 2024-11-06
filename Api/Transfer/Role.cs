using System;
using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;
public record RoleDetailDto(
    Hrib Id,
    Hrib OrganizationId,
    LocalizedString Name,
    LocalizedString? Description,
    DateTimeOffset CreatedOn,
    ImmutableDictionary<Hrib, ImmutableArray<Permission>> Permissions
);

public record RoleListDto(
    Hrib Id,
    Hrib OrganizationId,
    LocalizedString Name
);

public record RoleCreationDto(
    Hrib OrganizationId,
    LocalizedString Name,
    LocalizedString? Description
    // NB: Permissions are edited using the EntityPermissionsEditEndpoint

);

public record RoleEditDto(
    Hrib Id,
    LocalizedString Name,
    LocalizedString? Description
    // NB: Permissions are edited using the EntityPermissionsEditEndpoint
);

