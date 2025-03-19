using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Kafe;

public class ShardTypeRegistry : IFreezable
{
    private readonly ConcurrentDictionary<KafeType, ShardTypeMetadata> shards = new();

    public bool IsFrozen { get; private set; }

    public IReadOnlyDictionary<KafeType, ShardTypeMetadata> Shards { get; }

    public ShardTypeRegistry()
    {
        Shards = shards.AsReadOnly();
    }

    public void Freeze()
    {
        IsFrozen = true;
    }

    public ShardTypeRegistry Register(ShardTypeMetadata metadata)
    {
        AssertUnfrozen();
        if (!shards.TryAdd(metadata.KafeType, metadata))
        {
            throw new ArgumentException(
                $"Shard type '{metadata.KafeType}' has been already registered.",
                nameof(metadata)
            );
        }
        return this;
    }

    private void AssertUnfrozen()
    {
        if (IsFrozen)
        {
            throw new InvalidOperationException("This shard type registry is frozen and can no longer be modified.");
        }
    }
}
