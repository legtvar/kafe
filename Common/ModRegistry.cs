using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Kafe;

public class ModRegistry : IFreezable
{
    private readonly ConcurrentDictionary<string, ModMetadata> mods = new();
    private readonly KafeTypeRegistry typeRegistry;
    private readonly RequirementRegistry requirementRegistry;

    public bool IsFrozen { get; private set; }

    public IReadOnlyDictionary<string, ModMetadata> Mods { get; }

    public ModRegistry(
        KafeTypeRegistry typeRegistry,
        RequirementRegistry requirementRegistry
    )
    {
        Mods = mods.AsReadOnly();
        this.typeRegistry = typeRegistry;
        this.requirementRegistry = requirementRegistry;
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
        var modContext = new ModContext(
            name: modName,
            typeRegistry: typeRegistry,
            requirementRegistry: requirementRegistry
        );
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
