using System.Threading.Tasks;

namespace Kafe.Core.Requirements;

public abstract class ShardRequirementHandlerBase<TRequirement> : IShardRequirementHandler
        where TRequirement : IRequirement
{
    bool IRequirementHandler.CanHandle(IRequirement requirement)
    {
        return requirement is TRequirement;
    }

    async ValueTask IRequirementHandler.Handle(IRequirementContext<IRequirement> context)
    {
        var shard = await context.RequireShard() ?? default;
        if (shard is not null)
        {
            await Handle((IRequirementContext<TRequirement>)context, shard);
        }
    }


    ValueTask IShardRequirementHandler.Handle(
        IRequirementContext<IRequirement> context,
        IShard shard
    )
    {
        return Handle((IRequirementContext<TRequirement>)context, shard);
    }

    public abstract ValueTask Handle(IRequirementContext<TRequirement> context, IShard shard);
}
