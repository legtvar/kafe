using System;
using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;

public record ProjectListDto(
    Hrib Id,
    Hrib ProjectGroupId,
    LocalizedString Name,
    LocalizedString? Description,
    Visibility Visibility,
    DateTimeOffset ReleaseDate
    // TODO: Thumbnail
);

public record ProjectDetailDto(
    Hrib Id,
    Hrib ProjectGroupId,
    LocalizedString? ProjectGroupName,
    LocalizedString? Genre,
    LocalizedString Name,
    LocalizedString? Description,
    Visibility Visibility,
    DateTimeOffset ReleaseDate,
    ImmutableArray<ProjectAuthorDto> Crew,
    ImmutableArray<ProjectAuthorDto> Cast,
    ImmutableArray<ArtifactDetailDto> Artifacts
);

public record ProjectAuthorDto(
    string Id,
    string Name,
    ImmutableArray<string> Roles
);

public record ProjectCreationDto(
    string ProjectGroupId,
    LocalizedString Name,
    LocalizedString? Description,
    LocalizedString? Genre,
    ImmutableArray<ProjectCreationAuthorDto> Crew,
    ImmutableArray<ProjectCreationAuthorDto> Cast
);

public record ProjectCreationAuthorDto(
    string Id,
    ImmutableArray<string> Roles
);
