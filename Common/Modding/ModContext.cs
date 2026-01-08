using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe;

public sealed record ModContext
{
    private readonly HashSet<KafeType> types = [];

    public ModContext(
        string name,
        KafeTypeRegistry typeRegistry,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        IServiceCollection services
    )
    {
        Types = new ReadOnlySet<KafeType>(types);
        Name = name;
        TypeRegistry = typeRegistry;
        Configuration = configuration;
        HostEnvironment = hostEnvironment;
        Services = services;
    }

    /// <summary>
    /// Short, dash-case name of the mod. Taken from <see cref="IMod.Moniker"/>.
    /// </summary>
    public string Name { get; }

    public KafeTypeRegistry TypeRegistry { get; }

    public IConfiguration Configuration { get; }

    public IHostEnvironment HostEnvironment { get; }

    public IServiceCollection Services { get; }

    /// <summary>
    /// Types that were registered by this mod through the <see cref="AddType"/> subtype registration methods.
    /// </summary>
    public IReadOnlySet<KafeType> Types { get; }
}
