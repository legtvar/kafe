using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe;

public sealed class RequirementContext<T> : IRequirementContext<T>
    where T : IRequirement
{
    public RequirementContext(
        T requirement,
        KafeObject target,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken
    )
    {
        ServiceProvider = serviceProvider;
        TypeRegistry = serviceProvider.GetRequiredService<KafeTypeRegistry>();
        Requirement = requirement;
        RequirementType = TypeRegistry.RequireType<T>();
        Target = target.Value;
        TargetType = target.Type;
        CancellationToken = cancellationToken;
    }

    public IServiceProvider ServiceProvider { get; }
    public KafeTypeRegistry TypeRegistry { get; }
    public List<Diagnostic> Diagnostics { get; } = [];
    public T Requirement { get; }
    public KafeType RequirementType { get; }
    public CancellationToken CancellationToken { get; }
    public object Target { get; }
    public KafeType TargetType { get; }
}
