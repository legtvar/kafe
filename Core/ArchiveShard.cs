namespace Kafe.Core;

public record ArchiveShard : IShardMetadata
{
    public ArchiveKind ArchiveKind { get; init; }
}
