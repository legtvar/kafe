using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Kafe.Core.Diagnostics;

public static class DiagnosticFactoryExtensions
{
    public static Diagnostic Aggregate(
        this DiagnosticFactory f,
        params IEnumerable<Diagnostic> diagnostics
    )
    {
        var inner = diagnostics.ToImmutableArray();
        if (inner.Length == 0)
        {
            throw new ArgumentException(
                "At least one diagnostic is required to construct an aggregate diagnostic.",
                nameof(diagnostics)
            );
        }
        if (inner.Length == 1)
        {
            return inner[0];
        }

        var severity = inner.Max(i => i.Severity);
        return f.FromPayload(new AggregateDiagnostic(inner), severity);
    }

    /// <summary>
    /// Creates a regular or an <see cref="AggregateDiagnostic"/> based on the length of 
    /// <paramref name="diagnosticPayloads"/>.
    /// </summary>
    public static Diagnostic FromManyPayloads(
        this DiagnosticFactory f,
        params IEnumerable<object> diagnosticPayloads
    )
    {
        return f.Aggregate(diagnosticPayloads.Select(m => f.FromPayload(m)));
    }

    public static Diagnostic ForParameter(
        this DiagnosticFactory f,
        string parameterName,
        params IEnumerable<Diagnostic> parameterDiagnostics
    )
    {
        var inner = f.Aggregate(parameterDiagnostics);
        return f.FromPayload(new ParameterDiagnostic(parameterName, inner));
    }

    public static Diagnostic ForParameter(
        this DiagnosticFactory f,
        string parameterName,
        params IEnumerable<object> parameterDiagnosticPayloads
    )
    {
        return f.ForParameter(parameterName, parameterDiagnosticPayloads.Select(m => f.FromPayload(m)));
    }

    public static Diagnostic NotFound(
        this DiagnosticFactory f,
        Type objectType,
        Hrib id,
        DiagnosticSeverity? severityOverride = null
    )
    {
        return f.FromPayload(new NotFoundDiagnostic(
            EntityType: f.TypeRegistry.RequireType(objectType),
            Id: id
        ), severityOverride);
    }

    public static Diagnostic NotFound<T>(
        this DiagnosticFactory f,
        Hrib id,
        DiagnosticSeverity? severityOverride = null
    )
    {
        return f.NotFound(typeof(T), id, severityOverride);
    }

    public static Diagnostic Unmodified(
        this DiagnosticFactory f,
        Type objectType,
        Hrib id,
        DiagnosticSeverity? severityOverride = null
    )
    {
        return f.FromPayload(new UnmodifiedDiagnostic(
            EntityType: f.TypeRegistry.RequireType(objectType),
            Id: id
        ), severityOverride);
    }

    public static Diagnostic Unmodified<T>(
        this DiagnosticFactory f,
        Hrib id,
        DiagnosticSeverity? severityOverride = null
    )
    {
        return f.Unmodified(typeof(T), id, severityOverride);
    }
    
    public static Diagnostic AlreadyExists(
        this DiagnosticFactory f,
        Type objectType,
        Hrib id,
        DiagnosticSeverity? severityOverride = null
    )
    {
        return f.FromPayload(new AlreadyExistsDiagnostic(
            EntityType: f.TypeRegistry.RequireType(objectType),
            Id: id
        ), severityOverride);
    }

    public static Diagnostic AlreadyExists<T>(
        this DiagnosticFactory f,
        Hrib id,
        DiagnosticSeverity? severityOverride = null
    )
    {
        return f.AlreadyExists(typeof(T), id, severityOverride);
    }
    
    public static Diagnostic Locked(
        this DiagnosticFactory f,
        Type objectType,
        Hrib id,
        DiagnosticSeverity? severityOverride = null
    )
    {
        return f.FromPayload(new LockedDiagnostic(
            EntityType: f.TypeRegistry.RequireType(objectType),
            Id: id
        ), severityOverride);
    }

    public static Diagnostic Locked<T>(
        this DiagnosticFactory f,
        Hrib id,
        DiagnosticSeverity? severityOverride = null
    )
    {
        return f.Locked(typeof(T), id, severityOverride);
    }
}
