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
        RequirementTypeRegistry requirementTypeRegistry,
        ShardTypeRegistry shardTypeRegistry,
        IServiceProvider services
    )
    {
        Types = new ReadOnlySet<KafeType>(types);
        Name = name;
        TypeRegistry = typeRegistry;
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

    public RequirementTypeRegistry RequirementTypeRegistry { get; }

    public ShardTypeRegistry ShardTypeRegistry { get; }

    public IServiceProvider Services { get; }

    public IReadOnlySet<KafeType> Types { get; }

    internal KafeType AddType(Type type, KafeTypeRegistrationOptions? options = null)
    {
        options ??= KafeTypeRegistrationOptions.Default;

        var name = options.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = type.Name;
            switch (options.Usage)
            {
                case KafeTypeUsage.ArtifactProperty:
                    name = Naming.WithoutSuffix(name, "Shard");
                    name = Naming.WithoutSuffix(name, "ShardMetadata");
                    break;
                case KafeTypeUsage.ShardMetadata:
                    name = Naming.WithoutSuffix(name, "Shard");
                    name = Naming.WithoutSuffix(name, "ShardMetadata");
                    break;
                case KafeTypeUsage.Requirement:
                    name = Naming.WithoutSuffix(name, "Requirement");
                    break;
            }
            name = Naming.ToDashCase(name);
        }

        string primary = name;
        string? secondary = null;
        switch (options.Usage)
        {
            case KafeTypeUsage.ShardMetadata:
                secondary = primary;
                primary = "shard";
                break;
            case KafeTypeUsage.Requirement:
                secondary = primary;
                primary = "requirement";
                break;
        }

        var kafeType = new KafeType(
            mod: Name,
            primary: primary,
            secondary: secondary,
            isArray: false);
        TypeRegistry.Register(
            new(
                KafeType: kafeType,
                DotnetType: type,
                Usage: options.Usage,
                Accessibility: options.Accessibility,
                DefaultRequirements: [.. options.DefaultRequirements],
                Converter: options.Converter
            )
        );
        types.Add(kafeType);
        return kafeType;
    }

    public KafeType AddArtifactProperty(Type propertyType, ArtifactPropertyRegistrationOptions? options = null)
    {
        options ??= ArtifactPropertyRegistrationOptions.Default;
        var kafeType = AddType(propertyType, new()
        {
            Accessibility = options.Accessibility,
            Converter = options.Converter,
            Name = options.Name,
            Usage = KafeTypeUsage.ArtifactProperty,
            DefaultRequirements = options.DefaultRequirements
        });
        return kafeType;
    }

    public KafeType AddRequirement(Type requirementType, RequirementRegistrationOptions? options = null)
    {
        options ??= RequirementRegistrationOptions.Default;

        var kafeType = AddType(requirementType, new()
        {
            Accessibility = options.Accessibility,
            Converter = options.Converter,
            Name = options.Name,
            Usage = KafeTypeUsage.Requirement
        });

        RequirementTypeRegistry.Register(new(
            KafeType: kafeType,
            HandlerTypes: [.. options.HandlerTypes]
        ));
        return kafeType;
    }

    public KafeType AddShard(Type shardType, ShardRegistrationOptions? options = null)
    {
        options ??= ShardRegistrationOptions.Default;

        var kafeType = AddType(shardType, new()
        {
            Accessibility = options.Accessibility,
            Converter = options.Converter,
            Name = options.Name,
            Usage = KafeTypeUsage.ShardMetadata
        });

        return kafeType;
    }

    public record KafeTypeRegistrationOptions
    {
        public static readonly KafeTypeRegistrationOptions Default = new();

        public KafeTypeUsage Usage { get; set; } = KafeTypeUsage.ArtifactProperty;

        public KafeTypeAccessibility Accessibility { get; set; } = KafeTypeAccessibility.Public;

        public string? Name { get; set; }

        public JsonConverter? Converter { get; set; }

        public List<IRequirement> DefaultRequirements { get; set; } = [];
    }

    public record ArtifactPropertyRegistrationOptions
    {
        public static readonly ArtifactPropertyRegistrationOptions Default = new();

        public KafeTypeAccessibility Accessibility { get; set; } = KafeTypeAccessibility.Public;

        public string? Name { get; set; }

        public JsonConverter? Converter { get; set; }
        public List<IRequirement> DefaultRequirements { get; set; } = [];
    }

    public record RequirementRegistrationOptions
    {
        public static readonly RequirementRegistrationOptions Default = new();

        public KafeTypeAccessibility Accessibility { get; set; } = KafeTypeAccessibility.Public;

        public string? Name { get; set; }

        public JsonConverter? Converter { get; set; }

        public List<Type> HandlerTypes { get; set; } = [];
    }

    public record ShardRegistrationOptions
    {
        public static readonly ShardRegistrationOptions Default = new();

        public KafeTypeAccessibility Accessibility { get; set; } = KafeTypeAccessibility.Public;

        public string? Name { get; set; }

        public JsonConverter? Converter { get; set; }
    }
}
