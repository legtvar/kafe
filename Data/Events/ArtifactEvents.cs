using System;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record ArtifactCreated(
    [Hrib] string ArtifactId,
    CreationMethod CreationMethod,
    [LocalizedString] ImmutableDictionary<string, string> Name,
    DateTimeOffset AddedOn
);

public record ArtifactInfoChanged(
    [Hrib] string ArtifactId,
    [LocalizedString] ImmutableDictionary<string, string>? Name,
    DateTimeOffset? AddedOn
);
