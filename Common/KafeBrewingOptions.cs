using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe;

public class KafeBrewingOptions
{
    private readonly Dictionary<Type, ISubtypeRegistry> subtypeRegistries = [];
    private readonly List<IMod> mods = [];
    private readonly List<IKafeFormatter> formatters = [];
    private readonly IServiceCollection services;

    public KafeBrewingOptions(IServiceCollection services)
    {
        SubtypeRegistries = subtypeRegistries.AsReadOnly();
        Mods = mods.AsReadOnly();
        Formatters = formatters.AsReadOnly();
        this.services = services;
    }

    public IReadOnlyDictionary<Type, ISubtypeRegistry> SubtypeRegistries { get; }

    public IReadOnlyList<IMod> Mods { get; }

    public IReadOnlyList<IKafeFormatter> Formatters { get; }

    public KafeBrewingOptions AddSubtypeRegistry<TMetadata>(ISubtypeRegistry<TMetadata> subtypeRegistry)
        where TMetadata : class, ISubtypeMetadata
    {
        if (!subtypeRegistries.TryAdd(typeof(TMetadata), subtypeRegistry))
        {
            throw new InvalidOperationException(
                $"A KAFE subtype registry for metadata type '{typeof(TMetadata)}' has already been registered.");
        }
        services.AddSingleton<ISubtypeRegistry<TMetadata>>(subtypeRegistry);
        services.AddSingleton(subtypeRegistries.GetType(), subtypeRegistry);

        return this;
    }

    public KafeBrewingOptions AddMod<TMod>()
        where TMod : IMod, new()
    {
        var mod = new TMod();
        services.AddSingleton<IMod>(mod);
        services.AddSingleton(typeof(TMod), mod);
        mods.Add(mod);
        return this;
    }

    public KafeBrewingOptions AddFormatter(IKafeFormatter formatter)
    {
        formatters.Add(formatter);
        return this;
    }
}
