using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;
public record EntityPermissionsDetailDto(
    string Id,
    string? EntityType,
    ImmutableArray<Permission>? GlobalPermissions,
    ImmutableArray<Permission>? UserPermissions,
    ImmutableDictionary<string, ImmutableArray<Permission>> AccountPermissions
);
