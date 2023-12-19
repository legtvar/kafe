using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;

public record PlaylistListDto(
    Hrib Id,
    LocalizedString Name,
    LocalizedString? Description,
    Permission GlobalPermissions);

public record PlaylistDetailDto(
    Hrib Id,
    LocalizedString Name,
    LocalizedString? Description,
    Permission GlobalPermissions,
    ImmutableArray<PlaylistEntryDto> Entries);

public record PlaylistEntryDto(
    Hrib Id,
    LocalizedString Name);
