using System;

namespace Kafe.Data.Events;

public record ArtifactCreated(
    Hrib ArtifactId,
    CreationMethod CreationMethod,
    LocalizedString Name,
    DateTimeOffset AddedOn
);

public record ArtifactInfoChanged(
    Hrib ArtifactId,
    LocalizedString? Name,
    DateTimeOffset? AddedOn
);
