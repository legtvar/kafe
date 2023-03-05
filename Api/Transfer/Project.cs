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
    DateTimeOffset ReleasedOn
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
    DateTimeOffset ReleasedOn,
    ImmutableArray<ProjectAuthorDto> Crew,
    ImmutableArray<ProjectAuthorDto> Cast,
    ImmutableArray<ProjectArtifactDto> Artifacts,
    ProjectBlueprintDto Blueprint
);

public record ProjectAuthorDto(
    Hrib Id,
    string Name,
    ImmutableArray<string> Roles
);

public record ProjectArtifactDto(
    Hrib Id,
    LocalizedString Name,
    DateTimeOffset AddedOn,
    string? BlueprintSlot,
    ImmutableArray<ShardListDto> Shards
);

public record ProjectBlueprintDto(
    LocalizedString Name,
    ImmutableArray<ProjectArtifactBlueprintDto> ArtifactBlueprints
);

public record ProjectArtifactBlueprintDto(
    LocalizedString Name,
    string SlotName,
    ArgumentArity Arity,
    ImmutableArray<ProjectArtifactShardBlueprintDto> ShardBlueprints
);

public record ProjectArtifactShardBlueprintDto(
    LocalizedString Name,
    ShardKind Kind,
    ArgumentArity Arity
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
