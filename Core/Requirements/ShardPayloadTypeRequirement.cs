using System.Collections.Immutable;

namespace Kafe.Core.Requirements;

public record ShardPayloadTypeRequirement : IRequirement
{
    public static string Moniker => "shard-payload-type";

    public ShardPayloadTypeRequirement(
        ImmutableArray<KafeType> include = default,
        ImmutableArray<KafeType> exclude = default
    )
    {
        Include = include.IsDefaultOrEmpty ? [] : include;
        Exclude = exclude.IsDefaultOrEmpty ? [] : exclude;
    }

    public ImmutableArray<KafeType> Include { get; init; }
    public ImmutableArray<KafeType> Exclude { get; init; }
}
