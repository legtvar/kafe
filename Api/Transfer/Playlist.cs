using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;

public record PlaylistListDto(
    Hrib Id,
    Hrib OrganizationId,
    LocalizedString Name,
    LocalizedString? Description,
    ImmutableArray<Permission> GlobalPermissions);

public record PlaylistDetailDto(
    Hrib Id,
    Hrib OrganizationId,
    LocalizedString Name,
    LocalizedString? Description,
    ImmutableArray<Permission> GlobalPermissions,
    ImmutableArray<PlaylistEntryDto> Entries);

public record PlaylistEntryDto(
    Hrib Id,
    LocalizedString Name);

public record PlaylistCreationDto(
    LocalizedString Name,
    Hrib OrganizationId,
    LocalizedString? Description,
    ImmutableArray<Permission>? GlobalPermissions,
    ImmutableArray<string>? EntryIds);

public record PlaylistEditDto(
    Hrib Id,
    // TODO: Uncomment once moving groups between organizations is implemented.
    // Hrib? OrganizationId,
    LocalizedString? Name,
    LocalizedString? Description,
    ImmutableArray<Permission>? GlobalPermissions,
    ImmutableArray<string>? EntryIds);
