using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe;

public sealed record class ModContext
{
    private HashSet<KafeType> types = [];

    public ModContext(
        string name,
        KafeTypeRegistry typeRegistry,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        IReadOnlyDictionary<Type, ISubtypeRegistry> subtypeRegistries
    )
    {
        Types = new ReadOnlySet<KafeType>(types);
        Name = name;
        TypeRegistry = typeRegistry;
        Configuration = configuration;
        HostEnvironment = hostEnvironment;
        SubtypeRegistries = subtypeRegistries;
    }

    /// <summary>
    /// Short, dash-case name of the mod. Taken from <see cref="IMod.Name"/>.
    /// </summary>
    public string Name { get; }

    public KafeTypeRegistry TypeRegistry { get; }

    public IConfiguration Configuration { get; }

    public IHostEnvironment HostEnvironment { get; }

    public IReadOnlyDictionary<Type, ISubtypeRegistry> SubtypeRegistries { get; }

    public IServiceCollection Services { get; }

    /// <summary>
    /// Types that were registered by this mod through the <see cref="AddType"/> subtype registration methods.
    /// </summary>
    public IReadOnlySet<KafeType> Types { get; }

    public KafeType AddType(Type type, KafeTypeRegistrationOptions? options = null)
    {
        options ??= KafeTypeRegistrationOptions.Default;
        var typeName = options.Name;
        if (string.IsNullOrWhiteSpace(typeName))
        {
            typeName = Naming.ToDashCase(type.Name);
        }

        var kafeType = string.IsNullOrWhiteSpace(options.Subtype)
            ? new KafeType(
                mod: Name,
                primary: typeName,
                secondary: null
            )
            : new KafeType(
                mod: Name,
                primary: options.Subtype,
                secondary: typeName
            );

        TypeRegistry.Register(new(
            KafeType: kafeType,
            DotnetType: type,
            Accessibility: options.Accessibility,
            Converter: options.Converter
        ));
        return kafeType;
    }

    public record KafeTypeRegistrationOptions
    {
        public static readonly KafeTypeRegistrationOptions Default = new();

        public KafeTypeAccessibility Accessibility { get; set; } = KafeTypeAccessibility.Public;

        public string? Name { get; set; }

        public string? Subtype { get; set; }

        public JsonConverter? Converter { get; set; }
    }
}
