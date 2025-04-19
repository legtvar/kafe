using System;
using System.Diagnostics.CodeAnalysis;

namespace Kafe;

public static class RequirementContextExtensions
{
    // NB: This exists because Diagnostic? would otherwise fall into the payload overload which would error out
    public static IRequirementContext<T> Report<T>(
        this IRequirementContext<T> c,
        [DisallowNull] Diagnostic? diagnostic
    )
        where T : IRequirement
    {
        if (!diagnostic.HasValue)
        {
            throw new ArgumentNullException(nameof(diagnostic));
        }

        return c.Report(diagnostic.Value);
    }

    public static IRequirementContext<T> Report<T, TPayload>(
        this IRequirementContext<T> c,
        TPayload payload,
        DiagnosticSeverity? severityOverride = null
    )
        where T : IRequirement
        where TPayload : notnull
    {
        var diagnostic = c.DiagnosticFactory.FromPayload(payload, severityOverride);
        return c.Report(diagnostic);
    }
}
