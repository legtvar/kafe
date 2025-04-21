using System;
using System.Collections.Generic;
using System.Threading;

namespace Kafe;

public interface IRequirementContext<out T>
    where T : IRequirement
{
    List<Diagnostic> Diagnostics { get; }
    T Requirement { get; }
    KafeType RequirementType { get; }
    KafeObject Object { get; }
    IServiceProvider ServiceProvider { get; }
    KafeTypeRegistry TypeRegistry { get; }
    KafeObjectFactory KafeObjectFactory { get; }
    DiagnosticDescriptorRegistry DiagnosticDescriptorRegistry { get; }
    DiagnosticFactory DiagnosticFactory { get; }
    CancellationToken CancellationToken { get; }
}
