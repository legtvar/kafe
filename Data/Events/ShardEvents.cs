using System;

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

public record ShardPayloadSet(
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

[Obsolete("Use shard links instead.")]
public record ShardVariantAdded(
    [Hrib] string ShardId,
    string Name,
    KafeObject Metadata,
    ExistingValueHandling ExistingValueHandling
);

[Obsolete("Use shard links instead.")]
public record ShardVariantRemoved(
    [Hrib] string ShardId,
    string Name
);

public record ShardLinkAdded(
    [Hrib] string SourceShardId,
    [Hrib] string DestinationShardId,
    KafeObject LinkPayload
);

/// <summary>
/// Removes a shard link. <see cref="DestinationShardId"/> and <see cref="LinkPayload"/> can be used to specify which
/// links will be removed.
/// </summary>
public record ShardLinkRemoved(
    [Hrib] string SourceShardId,
    [Hrib] string? DestinationShardId,
    KafeObject? LinkPayload
);
