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

    public KafeType AddType(Type type, KafeTypeRegistrationOptions? options = null)
    {
        options ??= KafeTypeRegistrationOptions.Default;
        var typeName = options.Moniker;
        if (string.IsNullOrWhiteSpace(typeName))
        {
            typeName = Naming.ToDashCase(type.Name);
        }

        var kafeType = string.IsNullOrWhiteSpace(options.Subtype)
            ? new KafeType(
                mod: Name,
                primary: typeName,
                secondary: null,
                name: options.HumanReadableName
            )
            : new KafeType(
                mod: Name,
                primary: options.Subtype,
                secondary: typeName,
                name: options.HumanReadableName
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

        /// <summary>
        /// Short, dash-case name. Will be used for <see cref="KafeType.Primary"/> if <see cref="Subtype"/> is null or 
        /// empty, or for <see cref="KafeType.Secondary"/> if <see cref="Subtype"/> is defined.
        /// </summary>
        public string? Moniker { get; set; }

        /// <summary>
        /// The "subtype" or category of this type. Used for <see cref="KafeType.Primary"/>.
        /// This property is typically set automatically through <see cref="ModContext"/> extension methods.
        /// </summary>
        public string? Subtype { get; set; }

        public LocalizedString? HumanReadableName { get; set; }

        public JsonConverter? Converter { get; set; }
    }
}
