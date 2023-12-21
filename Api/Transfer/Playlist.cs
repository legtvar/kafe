using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;

public record PlaylistListDto(
    Hrib Id,
    LocalizedString Name,
    LocalizedString? Description,
    ImmutableArray<Permission> GlobalPermissions);

public record PlaylistDetailDto(
    Hrib Id,
    LocalizedString Name,
    LocalizedString? Description,
    ImmutableArray<Permission> GlobalPermissions,
    ImmutableArray<PlaylistEntryDto> Entries);

public record PlaylistEntryDto(
    Hrib Id,
    LocalizedString Name);

public record PlaylistCreationDto(
    LocalizedString Name,
    LocalizedString? Description,
    ImmutableArray<Permission>? GlobalPermissions,
    ImmutableArray<string>? EntryIds);

public record PlaylistEditDto(
    Hrib Id,
    LocalizedString? Name,
    LocalizedString? Description,
    ImmutableArray<Permission>? GlobalPermissions,
    ImmutableArray<string>? EntryIds);
