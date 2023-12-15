using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;
public record EntityPermissionsDetailDto(
    string Id,
    string? EntityType,
    ImmutableArray<Permission>? GlobalPermissions,
    ImmutableArray<Permission>? UserPermissions,
    ImmutableArray<EntityPermissionsAccountListDto> AccountPermissions
);

public record EntityPermissionsAccountListDto(
    string Id,
    string EmailAddress,
    ImmutableArray<Permission> Permissions
);

public record EntityPermissionsEditDto(
    string Id,
    ImmutableArray<Permission>? GlobalPermissions,
    ImmutableArray<EntityPermissionsAccountEditDto>? AccountPermissions
);

public record EntityPermissionsAccountEditDto(
    string? Id,
    string? EmailAddress,
    ImmutableArray<Permission> Permissions
);
