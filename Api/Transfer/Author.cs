using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;

public record AuthorListDto(
    string Id,
    string Name);

public record AuthorDetailDto(
    string Id,
    string Name,
    string? Uco,
    string? Email,
    string? Phone);
