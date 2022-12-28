using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Transfer;

public record PlaylistListDto(
    string Id,
    string? Name,
    string? Description,
    string? EnglishName,
    string? EnglishDescription,
    Visibility Visibility);

public record PlaylistDetailDto(
    string Id,
    string? Name,
    string? Description,
    string? EnglishName,
    string? EnglishDescription,
    Visibility? Visibility,
    ImmutableArray<string> Videos);
