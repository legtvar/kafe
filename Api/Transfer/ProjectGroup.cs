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
    ImmutableArray<ProjectListDto> Projects,
    ProjectValidationSettings? ValidationSettings
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
    Hrib? OrganizationId,
    LocalizedString? Name,
    LocalizedString? Description,
    DateTimeOffset? Deadline,
    bool? IsOpen,
    ProjectValidationSettings? ValidationSettings
);
