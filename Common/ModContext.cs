using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Collections.ObjectModel;

namespace Kafe;

public sealed record class ModContext
{
    private HashSet<KafeType> types = [];

    public ModContext(
        string name,
        KafeTypeRegistry typeRegistry,
        PropertyTypeRegistry propertyTypeRegistry,
        RequirementTypeRegistry requirementTypeRegistry,
        ShardTypeRegistry shardTypeRegistry,
        DiagnosticDescriptorRegistry diagnosticDescriptorRegistry,
        IServiceProvider services
    )
    {
        Types = new ReadOnlySet<KafeType>(types);
        Name = name;
        TypeRegistry = typeRegistry;
        PropertyTypeRegistry = propertyTypeRegistry;
        RequirementTypeRegistry = requirementTypeRegistry;
        ShardTypeRegistry = shardTypeRegistry;
        Services = services;
    }

    /// <summary>
    /// A short name for the mod (e.g., core, media, etc.).
    /// </summary>
    /// <remarks>
    /// May contain only lower-case letters, numbers, or '-'.
    /// Generated based on thre presence of a <see cref="ModAttribute"/> or generated automatically.
    /// </remarks>
    public string Name { get; }

    public KafeTypeRegistry TypeRegistry { get; }

    public PropertyTypeRegistry PropertyTypeRegistry { get; }

    public RequirementTypeRegistry RequirementTypeRegistry { get; }

    public ShardTypeRegistry ShardTypeRegistry { get; }

    public IServiceProvider Services { get; }

    public IReadOnlySet<KafeType> Types { get; }

    public KafeType AddProperty(Type propertyType, PropertyRegistrationOptions? options = null)
    {
        options ??= PropertyRegistrationOptions.Default;

        var name = options.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = propertyType.Name;
            name = Naming.WithoutSuffix(name, "Property");
            name = Naming.WithoutSuffix(name, "PropertyMetadata");
            name = Naming.ToDashCase(name);
        }

        var kafeType = new KafeType(
            mod: Name,
            primary: name,
            secondary: null,
            isArray: false
        );
        TypeRegistry.Register(new(
            KafeType: kafeType,
            DotnetType: propertyType,
            Accessibility: options.Accessibility,
            Converter: options.Converter
        ));
        PropertyTypeRegistry.Register(new(
            KafeType: kafeType,
            DefaultRequirements: [.. options.DefaultRequirements]
        ));
        return kafeType;
    }

    public KafeType AddRequirement(Type requirementType, RequirementRegistrationOptions? options = null)
    {
        options ??= RequirementRegistrationOptions.Default;

        var name = options.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = requirementType.Name;
            name = Naming.WithoutSuffix(name, "Requirement");
            name = Naming.ToDashCase(name);
        }

        var kafeType = new KafeType(
            mod: Name,
            primary: "requirement",
            secondary: name,
            isArray: false
        );
        TypeRegistry.Register(new(
            KafeType: kafeType,
            DotnetType: requirementType,
            Accessibility: options.Accessibility,
            Converter: options.Converter
        ));
        RequirementTypeRegistry.Register(new(
            KafeType: kafeType,
            HandlerTypes: [.. options.HandlerTypes]
        ));
        return kafeType;
    }

    public KafeType AddShard(Type shardType, ShardRegistrationOptions? options = null)
    {
        options ??= ShardRegistrationOptions.Default;

        var name = options.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = shardType.Name;
            name = Naming.WithoutSuffix(name, "Shard");
            name = Naming.WithoutSuffix(name, "ShardMetadata");
            name = Naming.ToDashCase(name);
        }

        var kafeType = new KafeType(
            mod: Name,
            primary: "shard",
            secondary: name,
            isArray: false
        );
        TypeRegistry.Register(new(
            KafeType: kafeType,
            DotnetType: shardType,
            Accessibility: options.Accessibility,
            Converter: options.Converter
        ));
        ShardTypeRegistry.Register(new(
            KafeType: kafeType,
            AnalyzerTypes: [.. options.AnalyzerTypes]
        ));
        return kafeType;
    }

    public abstract record KafeTypeRegistrationOptions
    {
        public KafeTypeAccessibility Accessibility { get; set; } = KafeTypeAccessibility.Public;

        public string? Name { get; set; }

        public JsonConverter? Converter { get; set; }
    }

    public record PropertyRegistrationOptions : KafeTypeRegistrationOptions
    {
        public static readonly PropertyRegistrationOptions Default = new();

        public List<IRequirement> DefaultRequirements { get; set; } = [];
    }

    public record RequirementRegistrationOptions : KafeTypeRegistrationOptions
    {
        public static readonly RequirementRegistrationOptions Default = new();

        public List<Type> HandlerTypes { get; set; } = [];
    }

    public record ShardRegistrationOptions : KafeTypeRegistrationOptions
    {
        public static readonly ShardRegistrationOptions Default = new();

        public List<Type> AnalyzerTypes { get; set; } = [];

    }
}
