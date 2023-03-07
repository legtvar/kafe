using System;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record ProjectCreated(
    [Hrib] string ProjectId,
    CreationMethod CreationMethod,
    string ProjectGroupId,
    [LocalizedString] ImmutableDictionary<string, string> Name,
    Visibility Visibility
);

public record ProjectAuthorAdded(
    [Hrib] string ProjectId,
    [Hrib] string AuthorId,
    ProjectAuthorKind Kind,
    ImmutableArray<string>? Roles = null
);

public record ProjectAuthorRemoved(
    [Hrib] string ProjectId,
    [Hrib] string AuthorId,
    ImmutableArray<string>? Roles = null
);

public record ProjectInfoChanged(
    [Hrib] string ProjectId,
    [LocalizedString] ImmutableDictionary<string, string>? Name = null,
    [LocalizedString] ImmutableDictionary<string, string>? Description = null,
    Visibility? Visibility = null,
    DateTimeOffset? ReleasedOn = null,
    [LocalizedString] ImmutableDictionary<string, string>? Genre = null
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
    Hrib ProjectId
);

public record ProjectReviewAdded(
    Hrib ProjectId,
    ReviewKind Kind,
    string ReviewerRole,
    LocalizedString? Comment
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
