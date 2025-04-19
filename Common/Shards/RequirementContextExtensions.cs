namespace Kafe;

public static class RequirementContextExtensions
{
    public IRequirementContext<T> Report<T, TPayload>(
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
