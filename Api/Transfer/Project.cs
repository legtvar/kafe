using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Transfer;

public record ProjectListDto(
    string Id,
    string ProjectGroupId,
    string? Name,
    string? Description,
    string? EnglishName,
    string? EnglishDescription,
    Visibility Visibility);

public record ProjectDetailDto(
    string Id,
    string ProjectGroupId,
    string? Name,
    string? Description,
    string? EnglishName,
    string? EnglishDescription,
    Visibility Visibility,
    ImmutableArray<string> Authors,
    DateTimeOffset ReleaseDate);
