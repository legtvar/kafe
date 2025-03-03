namespace Kafe.Core;

public record ShardTypeRequirement : IRequirement
{
    public KafeType ShardType { get; set; }
}
