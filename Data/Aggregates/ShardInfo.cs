using System;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Linq.CreatedAt;

namespace Kafe.Data.Aggregates;

public record ShardInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    [Hrib] string ArtifactId,
    DateTimeOffset CreatedAt,
    long Size,
    string Filename,
    KafeObject Metadata
)
{
    public static readonly ShardInfo Invalid = new();

    public ShardInfo() : this(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        ArtifactId: Hrib.InvalidValue,
        CreatedAt: default,
        Size: 0,
        Filename: Const.InvalidName,
        Metadata: KafeObject.Invalid
    )
    {
    }
}

public class ShardInfoProjection : SingleStreamProjection<ShardInfo>
{
    public ShardInfoProjection()
    {
    }

    public static ShardInfo Create(IEvent<ShardCreated> e)
    {
        return new(
            Id: e.Data.ShardId,
            CreationMethod: e.Data.CreationMethod,
            ArtifactId: e.Data.ArtifactId,
            CreatedAt: e.Timestamp,
            Size: e.Data.Size,
            Filename: e.Data.Filename,
            Metadata: e.Data.Metadata
        );
    }
}
