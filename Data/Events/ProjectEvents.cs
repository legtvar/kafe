using System;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record ProjectCreated(
    [Hrib] string ProjectId,
    [Hrib] string? OwnerId,
    CreationMethod CreationMethod,
    [Hrib] string ProjectGroupId,
    [Hrib] string ArtifactId
);

public record ProjectArtifactSet(
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

public record ProjectGlobalPermissionsChanged(
    [Hrib] string ProjectId,
    Permission GlobalPermissions
);
