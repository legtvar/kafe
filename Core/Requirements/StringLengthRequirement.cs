using System.Threading.Tasks;
using Kafe.Core.Diagnostics;

namespace Kafe.Core.Requirements;

/// <summary>
/// Ensures the length <see cref="string"/> or a <see cref="LocalizedString"/> is within (inclusive) limits.
/// </summary>
/// <param name="MinLength">Inclusive length minimum</param>
/// <param name="MaxLength">Inclusive length maximum</param>
public record StringLengthRequirement(
    int? MinLength,
    int? MaxLength
) : IRequirement
{
    public static string Moniker { get; } = "string-length";
}

public sealed class StringLengthRequirementHandler : RequirementHandlerBase<StringLengthRequirement>
{
    public override ValueTask Handle(IRequirementContext<StringLengthRequirement> context)
    {
        if (context.Requirement.MinLength is null && context.Requirement.MaxLength is null)
        {
            // TODO: warn about the uselessness of the user's doing.
            return ValueTask.CompletedTask;
        }

        if (context.Object.Value is not LocalizedString value)
        {
            if (context.Object.Value is string stringValue)
            {
                value = LocalizedString.CreateInvariant(stringValue);
            }
            else
            {
                context.Report(new IncompatibleRequirementDiagnostic(
                    context.TypeRegistry.RequireType<StringLengthRequirement>(),
                    context.Object.Type
                ));
                return ValueTask.CompletedTask;
            }
        }

        if (context.Requirement.MinLength is not null
            && LocalizedString.IsTooShort(value, context.Requirement.MinLength.Value))
        {
            context.Report(new StringTooShortDiagnostic(value, context.Requirement.MinLength.Value));
        }

        if (context.Requirement.MaxLength is not null
            && LocalizedString.IsTooLong(value, context.Requirement.MaxLength.Value))
        {
            context.Report(new StringTooLongDiagnostic(value, context.Requirement.MaxLength.Value));
        }

        return ValueTask.CompletedTask;
    }
}
