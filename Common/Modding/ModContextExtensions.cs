using System;
using System.Collections.Immutable;
using System.Configuration.Assemblies;
using System.Linq;
using System.Reflection;
using Kafe.Diagnostics;

namespace Kafe;

public static class ModContextExtensions
{
    public static KafeType AddArtifactProperty<T>(
        this ModContext c,
        ModContext.PropertyRegistrationOptions? options = null
    )
    {
        return c.AddProperty(typeof(T), options);
    }

    public static KafeType AddRequirement<T>(
        this ModContext c,
        ModContext.RequirementRegistrationOptions? options = null
    ) where T : IRequirement
    {
        return c.AddRequirement(typeof(T), options);
    }

    public static KafeType AddShard<T>(
        this ModContext c,
        ModContext.ShardRegistrationOptions? options = null
    )
    {
        return c.AddShard(typeof(T), options);
    }

    public static DiagnosticDescriptor AddDiagnostic<T>(
        this ModContext c,
        ModContext.DiagnosticDescriptorRegistrationOptions? options = null
    )
    {
        return c.AddDiagnostic(typeof(T), options);
    }

    public static ImmutableArray<DiagnosticDescriptor> AddDiagnosticFromAssembly(
        this ModContext c,
        Assembly assembly
    )
    {
        var payloadTypes = assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IDiagnosticPayload)))
            .ToImmutableArray();
        var builder = ImmutableArray.CreateBuilder<DiagnosticDescriptor>();
        foreach (var payloadType in payloadTypes)
        {
            builder.Add(c.AddDiagnostic(payloadType));
        }
        return builder.ToImmutable();
    }
}
