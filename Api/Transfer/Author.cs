using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;

public record AuthorListDto(
    string Id,
    string Name
);

public record AuthorDetailDto(
    string Id,
    string Name,
    LocalizedString? Bio,
    string? Uco,
    string? Email,
    string? Phone
);

public record AuthorCreationDto(
    string Name,
    LocalizedString? Bio,
    string? Uco,
    string? Email,
    string? Phone
);
