namespace Kafe.Data.Events;

public record ShardCreated(
    [Hrib] string ShardId,
    CreationMethod CreationMethod,
    long? FileLength,
    string? UploadFilename,
    string? MimeType,
    KafeObject Metadata
);

public record ShardMetadataSet(
    [Hrib] string ShardId,
    KafeObject Metadata,
    ExistingKafeObjectHandling ExistingValueHandling
);

public record ShardInfoChanged(
    [Hrib] string ShardId,
    long? FileLength,
    string? UploadFilename,
    string? MimeType
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
