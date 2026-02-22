using System;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record ProjectCreated(
    [Hrib] string ProjectId,
    [Hrib] string? OwnerId,
    CreationMethod CreationMethod,
    string ProjectGroupId,
    [LocalizedString] ImmutableDictionary<string, string> Name
);

public record ProjectAuthorAdded(
    [Hrib] string ProjectId,
    [Hrib] string AuthorId,
    ProjectAuthorKind Kind,
    ImmutableArray<string>? Roles = null
);

/// <summary>
/// Removes an author from a project.
/// </summary>
/// <param name="ProjectId">Hrib of the project.</param>
/// <param name="AuthorId">Hrib of the author to be removed.</param>
/// <param name="Kind">The kind of author. If null, removes from all kinds.</param>
/// <param name="Roles">The roles to remove. If null, removes author altogether.</param>
public record ProjectAuthorRemoved(
    [Hrib] string ProjectId,
    [Hrib] string AuthorId,
    ProjectAuthorKind? Kind = null,
    ImmutableArray<string>? Roles = null
);

public record ProjectInfoChanged(
    [Hrib] string ProjectId,
    [LocalizedString] ImmutableDictionary<string, string>? Name = null,
    [LocalizedString] ImmutableDictionary<string, string>? Description = null,
    DateTimeOffset? ReleasedOn = null,
    [LocalizedString] ImmutableDictionary<string, string>? Genre = null,
    string? AIUsageDeclaration = null,
    string? HearAboutUs = null
);

public record ProjectArtifactAdded(
    [Hrib] string ProjectId,
    [Hrib] string ArtifactId,
    string? BlueprintSlot
);

public record ProjectArtifactRemoved(
    [Hrib] string ProjectId,
    [Hrib] string ArtifactId
);

public record ProjectLocked(
    [Hrib] string ProjectId
);

public record ProjectUnlocked(
    [Hrib] string ProjectId
);

public record ProjectReviewAdded(
    [Hrib] string ProjectId,
    [Hrib] string? ReviewerId,
    ReviewKind Kind,
    string ReviewerRole,
    [LocalizedString] ImmutableDictionary<string, string>? Comment
);

//public record ProjectPassedAutomaticValidation(
//    Hrib ProjectId
//);

//public record ProjectFailedAutomaticValidation(
//    Hrib ProjectId,
//    LocalizedString Reason
//);

//public record ProjectPassedManualValidation(
//    Hrib ProjectId
//);

//public record ProjectFailedManualValidation(
//    Hrib ProjectId,
//    LocalizedString Reason
//);

//public record ProjectPassedDramaturgy(
//    Hrib ProjectId
//);

//public record ProjectFailedDramaturgy(
//    Hrib ProjectId,
//    LocalizedString Reason
//);

//public record ProjectValidationReset(
//    Hrib ProjectId
//);

public record ProjectGlobalPermissionsChanged(
    [Hrib] string ProjectId,
    Permission GlobalPermissions
);
