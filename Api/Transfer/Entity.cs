using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;
public record EntityPermissionsDetailDto(
    Hrib Id,
    string? EntityType,
    ImmutableArray<Permission>? GlobalPermissions,
    ImmutableArray<Permission>? UserPermissions,
    ImmutableArray<EntityPermissionsAccountListDto> AccountPermissions
);

public record EntityPermissionsAccountListDto(
    Hrib Id,
    string EmailAddress,
    ImmutableArray<Permission> Permissions
);

public record EntityPermissionsEditDto(
    Hrib Id,
    ImmutableArray<Permission>? GlobalPermissions,
    ImmutableArray<EntityPermissionsAccountEditDto>? AccountPermissions
);

public record EntityPermissionsAccountEditDto(
    Hrib? Id,
    string? EmailAddress,
    ImmutableArray<Permission> Permissions
);
