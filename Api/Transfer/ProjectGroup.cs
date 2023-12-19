using System;
using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;

public record ProjectGroupListDto(
    Hrib Id,
    LocalizedString Name,
    LocalizedString? Description,
    DateTimeOffset Deadline,
    bool IsOpen
);

public record ProjectGroupDetailDto(
    Hrib Id,
    LocalizedString Name,
    LocalizedString? Description,
    DateTimeOffset Deadline,
    bool IsOpen,
    ImmutableArray<ProjectListDto> Projects
);

public record ProjectGroupCreationDto(
    LocalizedString Name,
    LocalizedString? Description,
    DateTimeOffset Deadline
);

public record ProjectGroupEditDto(
    Hrib Id,
    LocalizedString? Name,
    LocalizedString? Description,
    DateTimeOffset? Deadline,
    bool? IsOpen
);
