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
            await Handle(new ShardRequirementContext<TRequirement>((IRequirementContext<TRequirement>)context, shard));
        }
    }


    ValueTask IShardRequirementHandler.Handle(
        IShardRequirementContext<IRequirement> context
    )
    {
        return Handle((IShardRequirementContext<TRequirement>)context);
    }

    public abstract ValueTask Handle(IShardRequirementContext<TRequirement> context);
}
