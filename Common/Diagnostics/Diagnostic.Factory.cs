using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Kafe;

public partial record struct Diagnostic
{
    public static Diagnostic Aggregate(params IEnumerable<Diagnostic> diagnostics)
    {
        return Aggregate(diagnostics.ToImmutableArray());
    }

    public static Diagnostic Aggregate(ImmutableArray<Diagnostic> diagnostics)
    {
        if (diagnostics.Length == 0)
        {
            throw new ArgumentException(
                "At least one diagnostic is required to construct an aggregate diagnostic.",
                nameof(diagnostics)
            );
        }
        if (diagnostics.Length == 1)
        {
            return diagnostics[0];
        }

        var severity = diagnostics.Max(i => i.Severity);
        return new Diagnostic(new AggregateDiagnostic(diagnostics), severityOverride: severity, skipFrames: 2);
    }

    public static Diagnostic Aggregate(params IEnumerable<IDiagnosticPayload> diagnosticPayloads)
    {
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        foreach (var payload in diagnosticPayloads)
        {
            diagnostics.Add(new Diagnostic(payload, skipFrames: 2));
        }

        return Aggregate(diagnostics.ToImmutable());
    }

    public static Diagnostic Fail(
        IDiagnosticPayload diagnosticPayload,
        DiagnosticDescriptor? descriptorOverride = null,
        int skipFrames = 1
    )
    {
        return new Diagnostic(
            payload: diagnosticPayload,
            descriptorOverride: descriptorOverride,
            severityOverride: DiagnosticSeverity.Error,
            skipFrames: skipFrames + 1
        );
    }
}
