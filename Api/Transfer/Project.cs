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
    ImmutableArray<ProjectReviewDto> Reviews,
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

public record ProjectReviewDto(
    ReviewKind Kind,
    string ReviewerRole,
    LocalizedString? Comment,
    DateTimeOffset AddedOn
);

public record ProjectReviewCreationDto(
    string ProjectId,
    ReviewKind Kind,
    string ReviewerRole,
    LocalizedString? Comment
);

public record ProjectBlueprintDto(
    LocalizedString Name,
    LocalizedString? Description,
    ImmutableArray<string> RequiredReviewers,
    ImmutableArray<ProjectArtifactBlueprintDto> ArtifactBlueprints
);

public record ProjectArtifactBlueprintDto(
    LocalizedString Name,
    LocalizedString? Description,
    string SlotName,
    ArgumentArity Arity,
    ImmutableArray<ProjectArtifactShardBlueprintDto> ShardBlueprints
);

public record ProjectArtifactShardBlueprintDto(
    LocalizedString Name,
    LocalizedString? Description,
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

public record ProjectValidationDto(
    Hrib ProjectId,
    DateTimeOffset ValidatedOn,
    ImmutableArray<ProjectDiagnosticDto> Diagnostics
);

public record ProjectDiagnosticDto(
    DiagnosticKind Kind,
    LocalizedString Message,
    string ValidationStage
);
