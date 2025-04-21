using System.Collections.Immutable;

namespace Kafe.Core.Requirements;

public record TypeRequirement(
    ImmutableArray<KafeType> Include,
    ImmutableArray<KafeType> Exclude
) : IRequirement
{
    public static string Moniker { get; } = "type";
}
