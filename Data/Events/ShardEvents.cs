namespace Kafe.Data.Events;

public record ShardCreated(
    [Hrib] string ShardId,
    CreationMethod CreationMethod,
    [Hrib] string ArtifactId,
    long Size,
    string Filename,
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
