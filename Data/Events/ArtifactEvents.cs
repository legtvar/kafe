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

public record ArtifactPropertiesSet(
    [Hrib] string ArtifactId,
    ImmutableDictionary<string, ArtifactPropertySetter> Properties
);

/// <summary>
/// Sets (or unsets) an artifact property.
/// </summary>
/// <param name="Object">Object or null. If null, unsets the property.</param>
public record ArtifactPropertySetter(
    KafeObject? Object,
    ExistingValueHandling ExistingValueHandling
);
