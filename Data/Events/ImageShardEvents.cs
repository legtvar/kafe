using Kafe.Media;

namespace Kafe.Data.Events;

public interface IImageShardEvent : IShardEvent
{
}

public record ImageShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    Hrib ArtifactId,
    ImageInfo OriginalVariantInfo
) : IImageShardEvent, IShardCreated;

public record ImageShardVariantsAdded(
    Hrib ShardId,
    string Name,
    ImageInfo Info
) : IImageShardEvent, IShardVariantAdded;

public record ImageShardVariantsRemoved(
    Hrib ShardId,
    string Name
) : IImageShardEvent, IShardVariantRemoved;
