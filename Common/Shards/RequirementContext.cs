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
        DiagnosticDescriptorRegistry descriptorRegistry,
        DiagnosticFactory diagnosticFactory
    )
    {
        Requirement = requirement;
        Object = @object;
        ServiceProvider = serviceProvider;
        TypeRegistry = typeRegistry;
        KafeObjectFactory = kafeObjectFactory;
        DiagnosticDescriptorRegistry = descriptorRegistry;
        DiagnosticFactory = diagnosticFactory;
    }

    public List<Diagnostic> Diagnostics { get; } = [];
    public IRequirement Requirement { get; }
    public KafeObject Object { get; set; }
    public IServiceProvider ServiceProvider { get; }
    public KafeTypeRegistry TypeRegistry { get; }
    public KafeObjectFactory KafeObjectFactory { get; }
    public DiagnosticDescriptorRegistry DiagnosticDescriptorRegistry { get; }
    public DiagnosticFactory DiagnosticFactory { get; }

    public RequirementContext Report(Diagnostic diagnostic)
    {
        Diagnostics.Add(diagnostic);
        return this;
    }

    public RequirementContext Report<TPayload>(TPayload payload, DiagnosticSeverity? severityOverride = null)
        where TPayload : notnull
    {
        var diagnostic = DiagnosticFactory.FromPayload(payload, severityOverride);
        return Report(diagnostic);
    }
}
