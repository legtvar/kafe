using System;
using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;

public record ProjectListDto(
    string Id,
    string ProjectGroupId,
    LocalizedString Name,
    LocalizedString? Description,
    Visibility Visibility,
    DateTimeOffset ReleaseDate
    // TODO: Thumbnail
);

public record ProjectDetailDto(
    string Id,
    string ProjectGroupId,
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
