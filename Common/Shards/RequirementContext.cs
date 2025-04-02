using System;
using System.Collections.Generic;

namespace Kafe;

public sealed class RequirementContext
{
    public RequirementContext(
        IRequirement requirement,
        KafeObject @object,
        IServiceProvider serviceProvider,
        KafeTypeRegistry typeRegistry,
        KafeObjectFactory kafeObjectFactory,
        DiagnosticDescriptorRegistry descriptorRegistry
    )
    {
        Requirement = requirement;
        Object = @object;
        ServiceProvider = serviceProvider;
        TypeRegistry = typeRegistry;
        KafeObjectFactory = kafeObjectFactory;
        DiagnosticDescriptorRegistry = descriptorRegistry;
    }

    public List<Diagnostic> Diagnostics { get; } = [];
    public IRequirement Requirement { get; }
    public KafeObject Object { get; set; }
    public IServiceProvider ServiceProvider { get; }
    public KafeTypeRegistry TypeRegistry { get; }
    public KafeObjectFactory KafeObjectFactory { get; }
    public DiagnosticDescriptorRegistry DiagnosticDescriptorRegistry { get; }

    public RequirementContext Report(Diagnostic diagnostic)
    {
        Diagnostics.Add(diagnostic);
        return this;
    }

    public RequirementContext Report<TPayload>(TPayload payload, DiagnosticSeverity? severityOverride = null)
        where TPayload : notnull
    {
        if (!TypeRegistry.DotnetTypeMap.TryGetValue(typeof(TPayload), out var kafeType))
        {
            throw new ArgumentException(
                $"No diagnostic type has a payload with .NET type '{typeof(TPayload)}'.",
                nameof(payload)
            );
        }

        if (!DiagnosticDescriptorRegistry.Descriptors.TryGetValue(kafeType, out var descriptor))
        {
            throw new ArgumentException(
                $".NET type '{typeof(TPayload)}' is registered as KAFE type '{kafeType}'. "
                    + $"However, '{kafeType}' is not a registered diagnostic descriptor."
            );
        }

        var payloadObject = KafeObjectFactory.Wrap(payload);
        var diagnostic = new Diagnostic(
            descriptor: descriptor,
            payload: payloadObject,
            severity: severityOverride,
            skipFrames: 2
        );
        return Report(diagnostic);
    }
}
