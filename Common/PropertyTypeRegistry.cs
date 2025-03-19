using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Kafe;

public class PropertyTypeRegistry
{
    private readonly ConcurrentDictionary<KafeType, PropertyTypeMetadata> requirements = new();

    public bool IsFrozen { get; private set; }

    public IReadOnlyDictionary<KafeType, PropertyTypeMetadata> Requirements { get; }

    public PropertyTypeRegistry()
    {
        Requirements = requirements.AsReadOnly();
    }

    public void Freeze()
    {
        IsFrozen = true;
    }

    public PropertyTypeRegistry Register(PropertyTypeMetadata metadata)
    {
        AssertUnfrozen();
        if (!requirements.TryAdd(metadata.KafeType, metadata))
        {
            throw new ArgumentException(
                $"Property type '{metadata.KafeType}' has been already registered.",
                nameof(metadata)
            );
        }
        return this;
    }

    private void AssertUnfrozen()
    {
        if (IsFrozen)
        {
            throw new InvalidOperationException("This property type registry is frozen and can no longer be modified.");
        }
    }
}
