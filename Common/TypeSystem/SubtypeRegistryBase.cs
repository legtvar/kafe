using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Kafe;

/// <summary>
/// Convenience class for the implementation of <see cref="ISubtypeRegistry{TMetadata}"/>.
/// </summary>
public abstract class SubtypeRegistryBase<TMetadata> : ISubtypeRegistry<TMetadata>
    where TMetadata : class, ISubtypeMetadata
{
    private readonly ConcurrentDictionary<KafeType, TMetadata> metadata = [];

    public SubtypeRegistryBase()
    {
        Metadata = metadata.AsReadOnly();
    }

    public IReadOnlyDictionary<KafeType, TMetadata> Metadata { get; }

    public bool IsFrozen { get; private set; }

    public void Freeze()
    {
        IsFrozen = true;
    }

    public void Register(TMetadata metadata)
    {
        AssertUnfrozen();
        if (!this.metadata.TryAdd(metadata.KafeType, metadata))
        {
            throw new ArgumentException(
                $"KAFE type '{metadata.KafeType}' has been already registered in this {GetType().Name}.",
                nameof(metadata)
            );
        }
    }

    private void AssertUnfrozen()
    {
        if (IsFrozen)
        {
            throw new InvalidOperationException($"This {GetType().Name} is frozen and can no longer be modified.");
        }
    }
}
