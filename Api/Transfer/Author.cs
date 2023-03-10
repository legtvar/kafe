using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;

public record AuthorListDto(
    string Id,
    string Name,
    Visibility Visibility
);

public record AuthorDetailDto(
    string Id,
    string Name,
    Visibility Visibility,
    LocalizedString? Bio,
    string? Uco,
    string? Email,
    string? Phone
);

public record AuthorCreationDto(
    string Name,
    Visibility Visibility,
    LocalizedString? Bio,
    string? Uco,
    string? Email,
    string? Phone
);
