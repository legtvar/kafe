namespace Kafe.Data.Events;

public interface IShardEvent
{
    [Hrib]
    string ShardId { get; }
}

public interface IShardCreated : IShardEvent
{
    CreationMethod CreationMethod { get; }

    [Hrib]
    string ArtifactId { get; }
}

public interface IShardVariantAdded : IShardEvent
{
    string Name { get; }
}

public interface IShardVariantRemoved : IShardEvent
{
    string Name { get; }
}

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
