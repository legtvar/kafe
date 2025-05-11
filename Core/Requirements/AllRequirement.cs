using System.Collections.Immutable;

namespace Kafe.Core.Requirements;

public record AllRequirement(
    ImmutableArray<KafeObject> Requirements
) : IRequirement
{
    public static string Moniker { get; } = "all";
}
