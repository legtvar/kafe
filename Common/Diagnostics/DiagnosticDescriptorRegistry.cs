using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Kafe;

public class DiagnosticDescriptorRegistry : SubtypeRegistryBase<DiagnosticDescriptor>
{
    public const string SubtypePrimary = "diag";

    public static readonly LocalizedString FallbackName = LocalizedString.Create(
        (Const.InvariantCulture, "diagnostic of type '{0}'"),
        (Const.CzechCulture, "hlášení typu '{0}'")
    );
}

public static class DiagnosticDescriptorModContextExtensions
{
    public const DiagnosticSeverity DefaultDiagnosticSeverity = DiagnosticSeverity.Error;

    public static DiagnosticDescriptor AddDiagnostic(
        this ModContext c,
        Type diagnosticPayloadType,
        DiagnosticDescriptorRegistrationOptions? options = null
    )
    {
        var diagnosticDescriptorRegistry = c.RequireSubtypeRegistry<DiagnosticDescriptor>();
        options ??= DiagnosticDescriptorRegistrationOptions.Default;
        options.Subtype ??= DiagnosticDescriptorRegistry.SubtypePrimary;

        if (diagnosticPayloadType.IsAssignableTo(typeof(IDiagnosticPayload)))
        {
            options.Name ??= diagnosticPayloadType.GetStaticPropertyValue<string>(
                propertyName: nameof(IDiagnosticPayload.Moniker),
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

        if (string.IsNullOrWhiteSpace(options.Name))
        {
            var typeName = diagnosticPayloadType.Name;
            typeName = Naming.WithoutSuffix(typeName, "DiagnosticDescriptor");
            typeName = Naming.WithoutSuffix(typeName, "Diagnostic");
            typeName = Naming.ToDashCase(typeName);
            options.Name = typeName;
        }

        options.HumanReadableName ??= LocalizedString.Format(
            DiagnosticDescriptorRegistry.FallbackName,
            options.Name
        );

        var kafeType = c.AddType(diagnosticPayloadType, options);
        var descriptor = new DiagnosticDescriptor()
        {
            Id = options.Name,
            KafeType = kafeType,
            DotnetType = diagnosticPayloadType,
            Title = options.Title ?? LocalizedString.CreateInvariant(kafeType.ToString()),
            Description = options.Description,
            HelpLinkUri = options.HelpLinkUri,
            MessageFormat = options.MessageFormat
                ?? LocalizedString.Format(DiagnosticDescriptor.FallbackMessageFormat, kafeType),
            DefaultSeverity = options.DefaultSeverity ?? DefaultDiagnosticSeverity
        };
        diagnosticDescriptorRegistry.Register(descriptor);
        return descriptor;
    }

    public static DiagnosticDescriptor AddDiagnostic<T>(
        this ModContext c,
        DiagnosticDescriptorRegistrationOptions? options = null
    )
    {
        return c.AddDiagnostic(typeof(T), options);
    }

    public static ImmutableArray<DiagnosticDescriptor> AddDiagnosticFromAssembly(
        this ModContext c,
        Assembly assembly
    )
    {
        var payloadTypes = assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IDiagnosticPayload)))
            .ToImmutableArray();
        var builder = ImmutableArray.CreateBuilder<DiagnosticDescriptor>();
        foreach (var payloadType in payloadTypes)
        {
            builder.Add(c.AddDiagnostic(payloadType));
        }
        return builder.ToImmutable();
    }

    public record DiagnosticDescriptorRegistrationOptions : ModContext.KafeTypeRegistrationOptions
    {
        public static new readonly DiagnosticDescriptorRegistrationOptions Default = new();

        public LocalizedString? Title { get; set; }

        public LocalizedString? Description { get; set; }

        public string? HelpLinkUri { get; set; }

        public LocalizedString? MessageFormat { get; set; }

        public DiagnosticSeverity? DefaultSeverity { get; set; }
    }
}
