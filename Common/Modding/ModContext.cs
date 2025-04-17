using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Collections.ObjectModel;
using System.Reflection;
using Kafe.Diagnostics;

namespace Kafe;

public sealed record class ModContext
{
    private HashSet<KafeType> types = [];
    
    public const DiagnosticSeverity DefaultDiagnosticSeverity = DiagnosticSeverity.Error;

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
        DiagnosticDescriptorRegistry = diagnosticDescriptorRegistry;
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
    public DiagnosticDescriptorRegistry DiagnosticDescriptorRegistry { get; }
    public IServiceProvider Services { get; }

    public IReadOnlySet<KafeType> Types { get; }

    public KafeType AddProperty(Type propertyType, PropertyRegistrationOptions? options = null)
    {
        options ??= PropertyRegistrationOptions.Default;

        var typeName = options.Name;
        if (string.IsNullOrWhiteSpace(typeName))
        {
            typeName = propertyType.Name;
            typeName = Naming.WithoutSuffix(typeName, "Property");
            typeName = Naming.WithoutSuffix(typeName, "PropertyMetadata");
            typeName = Naming.ToDashCase(typeName);
        }

        var kafeType = new KafeType(
            mod: Name,
            primary: typeName,
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

        var typeName = options.Name;
        if (string.IsNullOrWhiteSpace(typeName))
        {
            typeName = requirementType.Name;
            typeName = Naming.WithoutSuffix(typeName, "Requirement");
            typeName = Naming.ToDashCase(typeName);
        }

        var kafeType = new KafeType(
            mod: Name,
            primary: KafeType.RequirementPrimary,
            secondary: typeName,
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

        var typeName = options.Name;
        if (string.IsNullOrWhiteSpace(typeName))
        {
            typeName = shardType.Name;
            typeName = Naming.WithoutSuffix(typeName, "Shard");
            typeName = Naming.WithoutSuffix(typeName, "ShardMetadata");
            typeName = Naming.ToDashCase(typeName);
        }

        var kafeType = new KafeType(
            mod: Name,
            primary: KafeType.ShardPrimary,
            secondary: typeName,
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

    public DiagnosticDescriptor AddDiagnostic(
        Type diagnosticPayloadType,
        DiagnosticDescriptorRegistrationOptions? options = null
    )
    {
        options ??= DiagnosticDescriptorRegistrationOptions.Default;

        if (diagnosticPayloadType.IsAssignableTo(typeof(IDiagnosticPayload)))
        {
            options.Name ??= diagnosticPayloadType.GetStaticPropertyValue<string>(
                propertyName: nameof(IDiagnosticPayload.Name),
                isRequired: false,
                allowNull: true
            );
            options.Title ??= diagnosticPayloadType.GetStaticPropertyValue<LocalizedString>(
                propertyName: nameof(IDiagnosticPayload.Title),
                isRequired: false,
                allowNull: true
            );
            options.Description ??= diagnosticPayloadType.GetStaticPropertyValue<LocalizedString>(
                propertyName: nameof(IDiagnosticPayload.Description),
                isRequired: false,
                allowNull: true
            );
            options.MessageFormat ??= diagnosticPayloadType.GetStaticPropertyValue<LocalizedString>(
                propertyName: nameof(IDiagnosticPayload.MessageFormat),
                isRequired: false,
                allowNull: true
            );
            options.HelpLinkUri ??= diagnosticPayloadType.GetStaticPropertyValue<string>(
                propertyName: nameof(IDiagnosticPayload.HelpLinkUri),
                isRequired: false,
                allowNull: true
            );
            options.DefaultSeverity ??= diagnosticPayloadType.GetStaticPropertyValue<DiagnosticSeverity>(
                propertyName: nameof(IDiagnosticPayload.DefaultSeverity),
                isRequired: false,
                allowNull: true
            );
        }


        var typeName = options.Name ?? diagnosticPayloadType.Name;
        if (string.IsNullOrWhiteSpace(typeName))
        {
            typeName = diagnosticPayloadType.Name;
            typeName = Naming.WithoutSuffix(typeName, "DiagnosticDescriptor");
            typeName = Naming.WithoutSuffix(typeName, "Diagnostic");
            typeName = Naming.ToDashCase(typeName);
        }

        var kafeType = new KafeType(
            mod: Name,
            primary: KafeType.DiagnosticPrimary,
            secondary: typeName,
            isArray: false
        );
        TypeRegistry.Register(new(
            KafeType: kafeType,
            DotnetType: diagnosticPayloadType,
            Accessibility: options.Accessibility,
            Converter: options.Converter
        ));
        var descriptor = new DiagnosticDescriptor()
        {
            Id = typeName,
            KafeType = kafeType,
            DotnetType = diagnosticPayloadType,
            Title = options.Title ?? LocalizedString.CreateInvariant(kafeType.ToString()),
            Description = options.Description,
            HelpLinkUri = options.HelpLinkUri,
            MessageFormat = options.MessageFormat
                ?? LocalizedString.Format(DiagnosticDescriptor.FallbackMessageFormat, kafeType),
            DefaultSeverity = options.DefaultSeverity ?? DefaultDiagnosticSeverity
        };
        DiagnosticDescriptorRegistry.Register(descriptor);
        return descriptor;
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

    public record DiagnosticDescriptorRegistrationOptions : KafeTypeRegistrationOptions
    {
        public static readonly DiagnosticDescriptorRegistrationOptions Default = new();

        public LocalizedString? Title { get; set; }

        public LocalizedString? Description { get; set; }

        public string? HelpLinkUri { get; set; }

        public LocalizedString? MessageFormat { get; set; }

        public DiagnosticSeverity? DefaultSeverity { get; set; }
    }
}
