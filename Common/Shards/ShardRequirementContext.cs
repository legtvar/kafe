using System;
using System.Collections.Generic;
using System.Threading;

namespace Kafe;

public class ShardRequirementContext<T>(
    IRequirementContext<T> inner,
    IShard shard
) : IShardRequirementContext<T>
    where T : IRequirement
{
    public IRequirementContext<T> Inner { get; } = inner;

    public IShard Shard { get; } = shard;

    public List<Diagnostic> Diagnostics => Inner.Diagnostics;

    public T Requirement => Inner.Requirement;

    public object Target => Inner.Target;

    public IServiceProvider ServiceProvider => Inner.ServiceProvider;

    public CancellationToken CancellationToken => Inner.CancellationToken;
}
