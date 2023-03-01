using Kafe.Media;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Kafe.Data.Events;

public interface IVideoShardEvent : IShardEvent
{
}

public record VideoShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    Hrib ArtifactId,
    VideoShardVariant OriginalVariant
) : IVideoShardEvent, IShardCreated;

public record VideoShardVariantsAdded(
    Hrib ShardId,
    ImmutableArray<VideoShardVariant> Variants
) : IVideoShardEvent, IShardVariantsAdded
{
    public IEnumerable<string> GetVariantNames()
    {
        return Variants.Select(v => v.Name);
    }
}

public record VideoShardVariantsRemoved(
    Hrib ShardId,
    ImmutableArray<VideoShardVariant> Variants
) : IVideoShardEvent, IShardVariantsRemoved
{
    public IEnumerable<string> GetVariantNames()
    {
        return Variants.Select(v => v.Name);
    }
}
