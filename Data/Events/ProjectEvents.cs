using System;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record ProjectCreated(
    CreationMethod CreationMethod,
    string ProjectGroupId,
    LocalizedString Name,
    Visibility Visibility
);
public record ProjectAuthorAdded(
    string AuthorId,
    ProjectAuthorKind Kind,
    ImmutableArray<string>? Roles = null
);
public record ProjectAuthorRemoved(
    string AuthorId,
    ImmutableArray<string>? Roles = null
);
public record ProjectInfoChanged(
    LocalizedString? Name = null,
    LocalizedString? Description = null,
    Visibility? Visibility = null,
    DateTimeOffset? ReleaseDate = null,
    LocalizedString? Genre = null
);
public record ProjectArtifactAdded(
    string ArtifactId
);
public record ProjectArtifactRemoved(
    string ArtifactId
);
public record ProjectLocked;
public record ProjectUnlocked;
public record ProjectPassedAutomaticValidation;
public record ProjectFailedAutomaticValidation(
    LocalizedString Reason
);
public record ProjectPassedManualValidation;
public record ProjectFailedManualValidation(
    LocalizedString Reason
);
public record ProjectPassedDramaturgy;
public record ProjectFailedDramaturgy(
    LocalizedString Reason
);
public record ProjectValidationReset;
