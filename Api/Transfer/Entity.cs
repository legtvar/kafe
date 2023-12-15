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
