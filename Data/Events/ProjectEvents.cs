using System;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record ProjectCreated(
    Hrib ProjectId,
    CreationMethod CreationMethod,
    string ProjectGroupId,
    LocalizedString Name,
    Visibility Visibility
);

public record ProjectAuthorAdded(
    Hrib ProjectId,
    Hrib AuthorId,
    ProjectAuthorKind Kind,
    ImmutableArray<string>? Roles = null
);

public record ProjectAuthorRemoved(
    Hrib ProjectId,
    Hrib AuthorId,
    ImmutableArray<string>? Roles = null
);

public record ProjectInfoChanged(
    Hrib ProjectId,
    LocalizedString? Name = null,
    LocalizedString? Description = null,
    Visibility? Visibility = null,
    DateTimeOffset? ReleasedOn = null,
    LocalizedString? Genre = null
);

public record ProjectArtifactAdded(
    Hrib ProjectId,
    Hrib ArtifactId,
    string? BlueprintSlot
);

public record ProjectArtifactRemoved(
    Hrib ProjectId,
    Hrib ArtifactId
);

public record ProjectLocked(
    Hrib ProjectId
);

public record ProjectUnlocked(
    Hrib ProjectId
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
