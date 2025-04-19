using System;
using System.Collections.Generic;

namespace Kafe;

public interface IRequirementContext<out T>
    where T : IRequirement
{
    List<Diagnostic> Diagnostics { get; }
    T Requirement { get; }
    KafeObject Object { get; set; }
    IServiceProvider ServiceProvider { get; }
    KafeTypeRegistry TypeRegistry { get; }
    KafeObjectFactory KafeObjectFactory { get; }
    DiagnosticDescriptorRegistry DiagnosticDescriptorRegistry { get; }
    DiagnosticFactory DiagnosticFactory { get; }

    IRequirementContext<T> Report(Diagnostic diagnostic)
    {
        Diagnostics.Add(diagnostic);
        return this;
    }
}

public sealed class RequirementContext<T> : IRequirementContext<T>
    where T : IRequirement
{
    public RequirementContext(
        T requirement,
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
    public T Requirement { get; }
    public KafeObject Object { get; set; }
    public IServiceProvider ServiceProvider { get; }
    public KafeTypeRegistry TypeRegistry { get; }
    public KafeObjectFactory KafeObjectFactory { get; }
    public DiagnosticDescriptorRegistry DiagnosticDescriptorRegistry { get; }
    public DiagnosticFactory DiagnosticFactory { get; }
}
