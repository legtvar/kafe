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
    BlobInfo Info,
    string? UploadFileName = null
) : ShardInfoBase(Id, CreationMethod, ArtifactId, CreatedAt, UploadFileName)
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
            Info: e.Data.Info,
            UploadFileName: e.Data.UploadFileName
        );
    }
}
