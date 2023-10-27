using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;

public record AuthorListDto(
    string Id,
    string Name,
    Permission GlobalPermissions
);

public record AuthorDetailDto(
    string Id,
    string Name,
    Permission GlobalPermissions,
    LocalizedString? Bio,
    string? Uco,
    string? Email,
    string? Phone
);

public record AuthorCreationDto(
    string Name,
    Permission GlobalPermissions,
    LocalizedString? Bio,
    string? Uco,
    string? Email,
    string? Phone
);
