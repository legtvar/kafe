using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Kafe;

public static class RequirementContextExtensions
{
    public static TContext Report<TContext>(
        this TContext context,
        Diagnostic diagnostic
    )
        where TContext : IRequirementContext<IRequirement>
    {
        context.Diagnostics.Add(diagnostic);
        return context;
    }

    // NB: This exists because Diagnostic? would otherwise fall into the payload overload which would error out
    public static TContext Report<TContext>(
        this TContext c,
        [DisallowNull] Diagnostic? diagnostic
    )
        where TContext : IRequirementContext<IRequirement>
    {
        if (!diagnostic.HasValue)
        {
            throw new ArgumentNullException(nameof(diagnostic));
        }

        return c.Report(diagnostic.Value);
    }

    public static TContext Report<TContext, TPayload>(
        this TContext c,
        TPayload payload,
        DiagnosticSeverity? severityOverride = null
    )
        where TContext : IRequirementContext<IRequirement>
        where TPayload : notnull
    {
        var diagnostic = c.DiagnosticFactory.FromPayload(payload, severityOverride);
        return c.Report(diagnostic);
    }
}
