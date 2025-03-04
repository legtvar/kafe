using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Kafe;

public class RequirementRegistry : IFreezable
{
    private readonly ConcurrentDictionary<KafeType, RequirementMetadata> requirements = new();

    public bool IsFrozen { get; private set; }

    public IReadOnlyDictionary<KafeType, RequirementMetadata> Requirements { get; }

    public RequirementRegistry()
    {
        Requirements = requirements.AsReadOnly();
    }

    public void Freeze()
    {
        IsFrozen = true;
    }

    public RequirementRegistry Register(RequirementMetadata metadata)
    {
        AssertUnfrozen();
        if (!requirements.TryAdd(metadata.KafeType, metadata))
        {
            throw new ArgumentException(
                $"Requirement '{metadata.KafeType}' has been already registered.",
                nameof(metadata)
            );
        }
        return this;
    }

    private void AssertUnfrozen()
    {
        if (IsFrozen)
        {
            throw new InvalidOperationException("This IRequirement registry is frozen and can no longer be modified.");
        }
    }
}
