using System;
using System.Collections.Generic;
using System.Threading;
using Kafe;

namespace Kafe;

public class ShardRequirementContext<T> : IShardRequirementContext<T>
    where T : IRequirement
{
    public ShardRequirementContext(IRequirementContext<T> inner, IShard shard)
    {
        Inner = inner;
        Shard = shard;
    }

    public IRequirementContext<T> Inner { get; }

    public IShard Shard { get; }

    public List<Diagnostic> Diagnostics => Inner.Diagnostics;

    public T Requirement => Inner.Requirement;

    public KafeType RequirementType => Inner.RequirementType;

    public KafeObject Object => Inner.Object;

    public IServiceProvider ServiceProvider => Inner.ServiceProvider;

    public KafeTypeRegistry TypeRegistry => Inner.TypeRegistry;

    public KafeObjectFactory KafeObjectFactory => Inner.KafeObjectFactory;

    public DiagnosticDescriptorRegistry DiagnosticDescriptorRegistry => Inner.DiagnosticDescriptorRegistry;

    public DiagnosticFactory DiagnosticFactory => Inner.DiagnosticFactory;

    public CancellationToken CancellationToken => Inner.CancellationToken;
}
