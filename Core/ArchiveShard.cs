namespace Kafe.Core;

public record ArchiveShard : IShardPayload
{
    public ArchiveKind ArchiveKind { get; init; }
}
