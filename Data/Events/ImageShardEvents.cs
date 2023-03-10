using Kafe.Media;

namespace Kafe.Data.Events;

public interface IImageShardEvent : IShardEvent
{
}

public record ImageShardCreated(
    [Hrib] string ShardId,
    CreationMethod CreationMethod,
    [Hrib] string ArtifactId,
    ImageInfo OriginalVariantInfo
) : IImageShardEvent, IShardCreated;

public record ImageShardVariantsAdded(
    [Hrib] string ShardId,
    string Name,
    ImageInfo Info
) : IImageShardEvent, IShardVariantAdded;

public record ImageShardVariantsRemoved(
    [Hrib] string ShardId,
    string Name
) : IImageShardEvent, IShardVariantRemoved;
