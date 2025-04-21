using System;
using System.Collections.Generic;
using System.Threading;

namespace Kafe;

public sealed class RequirementContext<T> : IRequirementContext<T>
    where T : IRequirement
{
    public RequirementContext(
        T requirement,
        KafeObject @object,
        CancellationToken cancellationToken,
        IServiceProvider serviceProvider,
        KafeTypeRegistry typeRegistry,
        KafeObjectFactory kafeObjectFactory,
        DiagnosticDescriptorRegistry descriptorRegistry,
        DiagnosticFactory diagnosticFactory
    )
    {
        Requirement = requirement;
        Object = @object;
        CancellationToken = cancellationToken;
        ServiceProvider = serviceProvider;
        TypeRegistry = typeRegistry;
        KafeObjectFactory = kafeObjectFactory;
        DiagnosticDescriptorRegistry = descriptorRegistry;
        DiagnosticFactory = diagnosticFactory;

        RequirementType = typeRegistry.RequireType<T>();
    }

    public List<Diagnostic> Diagnostics { get; } = [];
    public T Requirement { get; }
    public CancellationToken CancellationToken { get; }
    public KafeType RequirementType { get; }
    public KafeObject Object { get; }
    public IServiceProvider ServiceProvider { get; }
    public KafeTypeRegistry TypeRegistry { get; }
    public KafeObjectFactory KafeObjectFactory { get; }
    public DiagnosticDescriptorRegistry DiagnosticDescriptorRegistry { get; }
    public DiagnosticFactory DiagnosticFactory { get; }
}
