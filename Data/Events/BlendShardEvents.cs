using Kafe.Media;

namespace Kafe.Data.Events;

public interface IBlendShardEvent : IShardEvent
{
}

public record BlendShardCreated(
    [Hrib] string ShardId,
    CreationMethod CreationMethod,
    [Hrib] string ArtifactId,
    BlendInfo OriginalVariantInfo
) : IBlendShardEvent, IShardCreated;

public record BlendShardVariantAdded(
    [Hrib] string ShardId,
    string Name,
    BlendInfo Info
) : IBlendShardEvent, IShardVariantAdded;

public record BlendShardVariantRemoved(
    [Hrib] string ShardId,
    string Name
) : IBlendShardEvent, IShardVariantRemoved;
