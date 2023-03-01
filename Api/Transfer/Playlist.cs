using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;

public record PlaylistListDto(
    string Id,
    LocalizedString Name,
    LocalizedString? Description,
    Visibility Visibility);

public record PlaylistDetailDto(
    string Id,
    LocalizedString Name,
    LocalizedString? Description,
    Visibility Visibility,
    ImmutableArray<string> Videos);
