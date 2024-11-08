using System;
using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;

public record ProjectGroupListDto(
    Hrib Id,
    Hrib OrganizationId,
    LocalizedString Name,
    LocalizedString? Description,
    DateTimeOffset Deadline,
    bool IsOpen
);

public record ProjectGroupDetailDto(
    Hrib Id,
    Hrib OrganizationId,
    LocalizedString Name,
    LocalizedString? Description,
    DateTimeOffset Deadline,
    bool IsOpen,
    ImmutableArray<ProjectListDto> Projects
);

public record ProjectGroupCreationDto(
    Hrib OrganizationId,
    LocalizedString Name,
    LocalizedString? Description,
    DateTimeOffset Deadline,
    bool IsOpen
);

public record ProjectGroupEditDto(
    Hrib Id,
    // TODO: Uncomment once moving groups between organizations is implemented.
    // Hrib? OrganizationId,
    LocalizedString? Name,
    LocalizedString? Description,
    DateTimeOffset? Deadline,
    bool? IsOpen
);
