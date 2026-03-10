using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe;

public sealed record class RequirementContext<T> : IRequirementContext<T>
    where T : IRequirement
{
    public RequirementContext(
        T requirement,
        KafeObject? target,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken
    )
    {
        ServiceProvider = serviceProvider;
        TypeRegistry = serviceProvider.GetRequiredService<KafeTypeRegistry>();
        Requirement = requirement;
        RequirementType = TypeRegistry.RequireType<T>();
        RawTarget = target;
        CancellationToken = cancellationToken;
    }

    public IServiceProvider ServiceProvider { get; }

    public KafeTypeRegistry TypeRegistry { get; }

    public List<Diagnostic> Diagnostics { get; init; } = [];

    public T Requirement { get; }

    public KafeType RequirementType { get; }

    public CancellationToken CancellationToken { get; }

    public KafeObject? RawTarget { get; }

    public object? Target => RawTarget?.Value;
}
