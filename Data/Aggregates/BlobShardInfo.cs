using JasperFx.Events;
using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events.Aggregation;
using System;

namespace Kafe.Data.Aggregates;

public record BlobShardInfo(
    [Hrib] string Id,
    string? FileName,
    CreationMethod CreationMethod,
    DateTimeOffset CreatedAt,
    [Hrib] string ArtifactId,
    BlobInfo Info
) : ShardInfoBase(Id, CreationMethod, ArtifactId, CreatedAt)
{
    public override ShardKind Kind => ShardKind.Blob;
}

public class BlobShardInfoProjection : SingleStreamProjection<BlobShardInfo, string>
{
    public static BlobShardInfo Create(IEvent<BlobShardCreated> e)
    {
        return new(
            Id: e.Data.ShardId,
            FileName: e.Data.FileName,
            CreationMethod: e.Data.CreationMethod,
            ArtifactId: e.Data.ArtifactId,
            CreatedAt: e.Timestamp,
            Info: e.Data.Info
        );
    }
}
