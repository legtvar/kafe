using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Kafe;

public class RequirementTypeRegistry : IFreezable
{
    private readonly ConcurrentDictionary<KafeType, RequirementTypeMetadata> requirements = new();

    public bool IsFrozen { get; private set; }

    public IReadOnlyDictionary<KafeType, RequirementTypeMetadata> Requirements { get; }

    public RequirementTypeRegistry()
    {
        Requirements = requirements.AsReadOnly();
    }

    public void Freeze()
    {
        IsFrozen = true;
    }

    public RequirementTypeRegistry Register(RequirementTypeMetadata metadata)
    {
        AssertUnfrozen();
        if (!requirements.TryAdd(metadata.KafeType, metadata))
        {
            throw new ArgumentException(
                $"Requirement type '{metadata.KafeType}' has been already registered.",
                nameof(metadata)
            );
        }
        return this;
    }

    private void AssertUnfrozen()
    {
        if (IsFrozen)
        {
            throw new InvalidOperationException("This requirement type registry is frozen "
                + "and can no longer be modified.");
        }
    }
}
