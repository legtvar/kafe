using System;

namespace Kafe.Data.Aggregates;

public abstract record ShardInfoBase(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    [Hrib] string ArtifactId,
    DateTimeOffset CreatedAt) : IShardEntity
{
    public abstract ShardKind Kind { get; }
}
