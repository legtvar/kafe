using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Kafe;

public class ModRegistry : IFreezable
{
    private readonly ConcurrentDictionary<string, ModMetadata> mods = new();
    private readonly KafeTypeRegistry typeRegistry;
    private readonly IConfiguration configuration;
    private readonly IHostEnvironment hostEnvironment;
    private readonly IReadOnlyDictionary<Type, ISubtypeRegistry> subtypeRegistries;

    public bool IsFrozen { get; private set; }

    public IReadOnlyDictionary<string, ModMetadata> Mods { get; }

    public ModRegistry(
        KafeTypeRegistry typeRegistry,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        IReadOnlyDictionary<Type, ISubtypeRegistry> subtypeRegistries
    )
    {
        Mods = mods.AsReadOnly();
        this.typeRegistry = typeRegistry;
        this.configuration = configuration;
        this.hostEnvironment = hostEnvironment;
        this.subtypeRegistries = subtypeRegistries;
    }

    public void Freeze()
    {
        IsFrozen = true;
    }

    public ModRegistry Register(
        IMod mod
    )
    {
        AssertUnfrozen();

        var modType = mod.GetType();
        var modName = (string?)modType.GetProperty(nameof(IMod.Name))?.GetValue(null);
        if (string.IsNullOrWhiteSpace(modName))
        {
            modName = modType.Name;
        }

        modName = Naming.WithoutSuffix(modName, "Mod");
        modName = Naming.ToDashCase(modName);

        // TODO: assert modName is in dash-case

        var modContext = new ModContext(
            name: modName,
            typeRegistry: typeRegistry,
            configuration: configuration,
            hostEnvironment: hostEnvironment,
            subtypeRegistries: subtypeRegistries
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
