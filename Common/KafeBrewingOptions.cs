using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe;

public class KafeBrewingOptions
{
    private readonly List<IMod> mods = [];
    private readonly List<IKafeFormatter> formatters = [];
    private readonly IServiceCollection services;

    public KafeBrewingOptions(IServiceCollection services)
    {
        Mods = mods.AsReadOnly();
        Formatters = formatters.AsReadOnly();
        this.services = services;
    }

    public IReadOnlyList<IMod> Mods { get; }

    public IReadOnlyList<IKafeFormatter> Formatters { get; }

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
