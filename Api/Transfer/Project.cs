using System;
using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Api.Transfer;

/// <summary>
/// A DTO of a project. To be used when listing projects.
/// </summary>
/// <param name="GlobalPermissions">Permissions that apply to all users, even the anonymous ones.</param>
/// <param name="UserPermissions">
/// Permissions that apply to the currently logged in user. Includes the global permissions.
/// </param>
public record ProjectListDto(
    Hrib Id,
    Hrib ProjectGroupId,
    LocalizedString Name,
    LocalizedString? Description,
    ImmutableArray<Permission> GlobalPermissions,
    ImmutableArray<Permission> UserPermissions,
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
    ImmutableArray<Permission> GlobalPermissions,
    ImmutableArray<Permission> UserPermissions,
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
    ImmutableDictionary<string, ProjectArtifactBlueprintDto> ArtifactBlueprints
);

public record ProjectArtifactBlueprintDto(
    LocalizedString Name,
    LocalizedString? Description,
    ArgumentArity Arity,
    ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprintDto> ShardBlueprints
);

public record ProjectArtifactShardBlueprintDto(
    LocalizedString Name,
    LocalizedString? Description,
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

public record ProjectEditDto(
    string Id,
    LocalizedString? Name,
    LocalizedString? Description,
    LocalizedString? Genre,
    ImmutableArray<ProjectCreationAuthorDto>? Crew,
    ImmutableArray<ProjectCreationAuthorDto>? Cast,
    ImmutableArray<ProjectArtifactAdditionDto>? Artifacts
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

public record ProjectArtifactAdditionDto(
    string Id,
    string? BlueprintSlot
);
