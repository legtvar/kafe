using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Kafe;

public sealed record class ModContext
{
    private ImmutableHashSet<KafeType>.Builder types = ImmutableHashSet.CreateBuilder<KafeType>();

    public ModContext(
        string name,
        KafeTypeRegistry typeRegistry,
        RequirementRegistry requirementRegistry
    )
    {
        Name = name;
        TypeRegistry = typeRegistry;
        RequirementRegistry = requirementRegistry;
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

    public RequirementRegistry RequirementRegistry { get; }

    public ModContext AddType(Type type, KafeTypeRegistrationOptions? options = null)
    {
        options ??= KafeTypeRegistrationOptions.Default;

        var kafeType = new KafeType(
            mod: Name,
            primary: options.Name ?? Naming.ToDashCase(type.Name),
            secondary: null,
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
        return this;
    }

    public ModContext AddRequirement(Type requirementType, RequirementRegistrationOptions? options = null)
    {
        options ??= RequirementRegistrationOptions.Default;

        var kafeType = new KafeType(
            mod: Name,
            primary: "requirement",
            secondary: options.Name ?? Naming.ToDashCase(Naming.WithoutSuffix(requirementType.Name, "Requirement")),
            isArray: false
        );
        TypeRegistry.Register(new(
            KafeType: kafeType,
            DotnetType: requirementType,
            Usage: KafeTypeUsage.Requirement,
            Accessibility: options.Accessibility,
            DefaultRequirements: [],
            Converter: options.Converter
        ));
        RequirementRegistry.Register(new(
            KafeType: kafeType,
            HandlerTypes: [.. options.HandlerTypes]
        ));
        types.Add(kafeType);
        return this;
    }

    public ModMetadata BuildMetadata()
    {
        return new ModMetadata
        {
            Name = Name,
            Types = types.ToImmutable()
        };
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

    public record RequirementRegistrationOptions
    {
        public static readonly RequirementRegistrationOptions Default = new();

        public KafeTypeAccessibility Accessibility { get; set; } = KafeTypeAccessibility.Public;

        public string? Name { get; set; }

        public JsonConverter? Converter { get; set; }

        public List<Type> HandlerTypes { get; set; } = [];
    }
}
