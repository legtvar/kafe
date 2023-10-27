using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;

public record PlaylistListDto(
    string Id,
    LocalizedString Name,
    LocalizedString? Description,
    Permission GlobalPermissions);

public record PlaylistDetailDto(
    string Id,
    LocalizedString Name,
    LocalizedString? Description,
    Permission GlobalPermissions,
    ImmutableArray<string> Videos);
