using System;
using System.Collections.Generic;
using System.Threading;

namespace Kafe;

public sealed class RequirementContext<T>(
    T requirement,
    object target,
    CancellationToken cancellationToken,
    IServiceProvider serviceProvider
)
    : IRequirementContext<T>
    where T : IRequirement
{
    public List<Diagnostic> Diagnostics { get; } = [];
    public T Requirement { get; } = requirement;
    public CancellationToken CancellationToken { get; } = cancellationToken;
    public object Target { get; } = target;
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
}
