using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe;

public class ModRegistry : IFreezable
{
    private readonly ConcurrentDictionary<string, ModMetadata> mods = new();
    private readonly IServiceProvider services;

    public bool IsFrozen { get; private set; }

    public IReadOnlyDictionary<string, ModMetadata> Mods { get; }

    public ModRegistry(IServiceProvider services)
    {
        Mods = mods.AsReadOnly();
        this.services = services;
    }

    public void Freeze()
    {
        IsFrozen = true;
    }

    public ModRegistry Register(IMod mod)
    {
        AssertUnfrozen();

        var modType = mod.GetType();
        var modAttribute = modType.GetCustomAttribute<ModAttribute>();
        var modName = modAttribute?.Name ?? Naming.ToDashCase(Naming.WithoutSuffix(modType.Name, "Mod"));
        var modContext = ActivatorUtilities.CreateInstance<ModContext>(services, modName);
        mod.Configure(modContext);
        var metadata = new ModMetadata(
            Instance: mod,
            Name: modName,
            Types: [.. modContext.Types]
        );
        if (!mods.TryAdd(modName, metadata))
        {
            throw new ArgumentException($"A mod named '{modName}' is already registered.");
        }
        return this;
    }

    private void AssertUnfrozen()
    {
        if (IsFrozen)
        {
            throw new InvalidOperationException("This mod registry is frozen and can no longer be modified.");
        }
    }
}
