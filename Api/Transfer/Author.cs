using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;

public record AuthorListDto(
    Hrib Id,
    string Name,
    Permission GlobalPermissions
);

public record AuthorDetailDto(
    Hrib Id,
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
