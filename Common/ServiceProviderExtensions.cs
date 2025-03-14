using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe;

public static class ServiceProviderExtensions
{
    public static IServiceProvider ConfigureKafeMods(this IServiceProvider s)
    {
        var mods = s.GetService<IEnumerable<IMod>>();
        if (mods is null)
        {
            return s;
        }

        var modRegistry = s.GetRequiredService<ModRegistry>();
        foreach (var mod in mods)
        {
            modRegistry.Register(mod);
        }

        return s;
    }
}
