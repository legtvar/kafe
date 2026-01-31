namespace Kafe.Data.Events;

public record ShardCreated(
    [Hrib] string ShardId,
    LocalizedString? Name,
    CreationMethod CreationMethod,
    long? FileLength,
    string? UploadFilename,
    string? MimeType,
    KafeObject Payload
);

public record ShardMetadataSet(
    [Hrib] string ShardId,
    KafeObject Payload,
    ExistingValueHandling ExistingValueHandling
);

public record ShardInfoChanged(
    [Hrib] string ShardId,
    LocalizedString? Name,
    long? FileLength,
    string? UploadFilename,
    string? MimeType
);

public record ShardVariantAdded(
    [Hrib] string ShardId,
    string Name,
    KafeObject Metadata,
    ExistingValueHandling ExistingValueHandling
);

public record ShardVariantRemoved(
    [Hrib] string ShardId,
    string Name
);

public record ShardLinkAdded(
    [Hrib] string Id,
    KafeObject Metadata
);

public record ShardLinkRemoved(
    [Hrib] string? Id,
    KafeObject? Metadata
);
