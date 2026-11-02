using Kafe.Media;

namespace Kafe.Data.Events;

public interface IGenerativeShardEvent : IShardEvent
{
}

public record GenerativeShardCreated(
    [Hrib] string ShardId,
    CreationMethod CreationMethod,
    [Hrib] string ArtifactId,
    GenerativeInfo OriginalVariantInfo
) : IGenerativeShardEvent, IShardCreated;

public record GenerativeShardVariantAdded(
    [Hrib] string ShardId,
    string Name,
    GenerativeInfo Info
) : IGenerativeShardEvent, IShardVariantAdded;

public record GenerativeShardVariantRemoved(
    [Hrib] string ShardId,
    string Name
) : IGenerativeShardEvent, IShardVariantRemoved;
