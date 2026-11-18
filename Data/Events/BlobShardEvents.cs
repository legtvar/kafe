using Kafe.Media;

namespace Kafe.Data.Events;

public interface IBlobShardEvent : IShardEvent
{
}

public record BlobShardCreated(
    [Hrib] string ShardId,
    string? FileName,
    CreationMethod CreationMethod,
    [Hrib] string ArtifactId,
    BlobInfo Info
) : IBlobShardEvent, IShardCreated;
