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
}
