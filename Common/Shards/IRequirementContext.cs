using System;
using System.Collections.Generic;
using System.Threading;

namespace Kafe;

public interface IRequirementContext<out T>
    where T : IRequirement
{
    List<Diagnostic> Diagnostics { get; }

    T Requirement { get; }

    /// <summary>
    /// The object upon which the requirement is imposed.
    /// </summary>
    object Target { get; }

    IServiceProvider ServiceProvider { get; }

    CancellationToken CancellationToken { get; }
}
