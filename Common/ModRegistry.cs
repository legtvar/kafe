using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Kafe;

public class ModRegistry : IFreezable
{
    private readonly ConcurrentDictionary<string, ModMetadata> mods = new();

    public bool IsFrozen { get; private set; }

    public IReadOnlyDictionary<string, ModMetadata> Mods { get; }

    public ModRegistry()
    {
        Mods = mods.AsReadOnly();
    }

    public void Freeze()
    {
        IsFrozen = true;
    }

    private void AssertUnfrozen()
    {
        if (IsFrozen)
        {
            throw new InvalidOperationException("This mod registry is frozen and can no longer be modified.");
        }
    }
}
