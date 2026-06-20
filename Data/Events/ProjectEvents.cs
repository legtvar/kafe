using System;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record ProjectCreated(
    [Hrib]
    string ProjectId,
    [Hrib]
    string? OwnerId,
    CreationMethod CreationMethod,
    [Hrib]
    string ProjectGroupId,
    [Hrib]
    string? ArtifactId
);

public record ProjectArtifactSet(
    [Hrib]
    string ProjectId,
    [Hrib]
    string ArtifactId
);

/// <summary>
/// Same as <see cref="ProjectArtifactSet"/> but with metadata about the artifact.
/// This event type is not stored in the DB but created on demand,
/// for example in <see cref="Aggregates.ProjectDetailProjection"/>.
/// </summary>
public record ProjectArtifactMetadataChanged(
    [Hrib]
    string ProjectId,
    [Hrib]
    string ArtifactId,
    LocalizedString? Name
);

public record ProjectLocked(
    [Hrib]
    string ProjectId
);

public record ProjectUnlocked(
    [Hrib]
    string ProjectId
);

public record ProjectReviewAdded(
    [Hrib]
    string ProjectId,
    [Hrib]
    string? ReviewerId,
    ReviewKind Kind,
    string ReviewerRole,
    [LocalizedString]
    ImmutableDictionary<string, string>? Comment
);

public record ProjectGlobalPermissionsChanged(
    [Hrib]
    string ProjectId,
    Permission GlobalPermissions
);
