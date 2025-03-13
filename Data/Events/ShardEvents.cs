namespace Kafe.Data.Events;

public record ShardCreated(
    [Hrib] string ShardId,
    CreationMethod CreationMethod,
    [Hrib] string ArtifactId,
    long? Size,
    string? Filename,
    KafeObject Metadata
);

public record ShardMetadataSet(
    [Hrib] string ShardId,
    KafeObject Metadata,
    ExistingKafeObjectHandling ExistingValueHandling
);

public record ShardInfoChanged(
    long Size,
    string Filename
);

public record ShardVariantAdded(
    [Hrib] string ShardId,
    string Name,
    KafeObject Metadata,
    ExistingKafeObjectHandling ExistingValueHandling
);

public record ShardVariantRemoved(
    [Hrib] string ShardId,
    string Name
);
