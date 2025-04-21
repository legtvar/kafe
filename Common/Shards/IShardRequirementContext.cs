namespace Kafe;

public interface IShardRequirementContext<out T> : IRequirementContext<T>
    where T : IRequirement
{
    IShard Shard { get; }
}
