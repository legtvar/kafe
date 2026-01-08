using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Kafe;

public static class RequirementContextExtensions
{
    extension<TContext>(TContext context)
        where TContext : IRequirementContext<IRequirement>
    {
        public TContext Report(
            Diagnostic diagnostic
        )
        {
            context.Diagnostics.Add(diagnostic);
            return context;
        }

        public TContext Report(
            IDiagnosticPayload diagnosticPayload
        )
        {
            return context.Report(new Diagnostic(diagnosticPayload, skipFrames: 2));
        }
    }
}
