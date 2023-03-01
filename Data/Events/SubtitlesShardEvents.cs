using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Kafe.Data.Events;

public interface ISubtitlesShardEvent : IShardEvent
{
}

public record SubtitlesShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    Hrib ArtifactId,
    SubtitlesShardVariant OriginalVariant
) : ISubtitlesShardEvent, IShardCreated;

public record SubtitlesShardVariantsAdded(
    Hrib ShardId,
    ImmutableArray<SubtitlesShardVariant> Variants
) : ISubtitlesShardEvent, IShardVariantsAdded
{
    public IEnumerable<string> GetVariantNames()
    {
        return Variants.Select(v => v.Name);
    }
}

public record SubtitlesShardVariantsRemoved(
    Hrib ShardId,
    ImmutableArray<SubtitlesShardVariant> Variants
) : ISubtitlesShardEvent, IShardVariantsRemoved
{
    public IEnumerable<string> GetVariantNames()
    {
        return Variants.Select(v => v.Name);
    }
}
