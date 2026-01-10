using System.Collections.Immutable;

namespace Kafe.Core.Requirements;

public record ArrayLengthRequirement(
    int? Min,
    int? Max
) : IRequirement
{
    public static string Moniker => "array-length";
}
