using System.Threading.Tasks;
using Kafe.Core.Diagnostics;

namespace Kafe.Core.Requirements;

/// <summary>
/// Ensures the length <see cref="string"/> or a <see cref="LocalizedString"/> is within (inclusive) limits.
/// </summary>
/// <param name="Min">Inclusive length minimum</param>
/// <param name="Max">Inclusive length maximum</param>
public record StringLengthRequirement(
    int? Min,
    int? Max
) : IRequirement
{
    public static string Moniker => "string-length";
}

public sealed class StringLengthRequirementHandler : RequirementHandlerBase<StringLengthRequirement>
{
    public override ValueTask Handle(IRequirementContext<StringLengthRequirement> context)
    {
        if (context.Requirement.Min is null && context.Requirement.Max is null)
        {
            // TODO: warn about the uselessness of the user's doing.
            return ValueTask.CompletedTask;
        }

        if (context.Target is not LocalizedString value)
        {
            if (context.Target is string stringValue)
            {
                value = LocalizedString.CreateInvariant(stringValue);
            }
            else
            {
                context.ReportIncompatible();
                return ValueTask.CompletedTask;
            }
        }

        if (context.Requirement.Min is not null
            && LocalizedString.IsTooShort(value, context.Requirement.Min.Value))
        {
            context.Report(new StringTooShortDiagnostic(value, context.Requirement.Min.Value));
        }

        if (context.Requirement.Max is not null
            && LocalizedString.IsTooLong(value, context.Requirement.Max.Value))
        {
            context.Report(new StringTooLongDiagnostic(value, context.Requirement.Max.Value));
        }

        return ValueTask.CompletedTask;
    }
}
