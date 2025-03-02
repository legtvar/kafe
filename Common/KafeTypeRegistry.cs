using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Kafe;

public class KafeTypeRegistry : IFreezable
{
    private readonly ConcurrentDictionary<KafeType, KafeTypeMetadata> types = new();

    public bool IsFrozen { get; private set; }

    public IReadOnlyDictionary<KafeType, KafeTypeMetadata> Types { get; }

    public KafeTypeRegistry()
    {
        Types = types.AsReadOnly();
    }

    public void Freeze()
    {
        IsFrozen = true;
    }

    public KafeTypeRegistry Register(KafeType type, KafeTypeMetadata metadata)
    {
        AssertUnfrozen();
        if (!types.TryAdd(type, metadata))
        {
            throw new ArgumentException($"KafeType '{type}' has been already registered.", nameof(type));
        }
        return this;
    }

    private void AssertUnfrozen()
    {
        if (IsFrozen)
        {
            throw new InvalidOperationException("This KafeType registry is frozen and can no longer be modified.");
        }
    }
}
