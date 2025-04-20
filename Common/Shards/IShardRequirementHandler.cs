using System.Threading.Tasks;

namespace Kafe;

public interface IShardRequirementHandler : IRequirementHandler
{
    ValueTask Handle(IRequirementContext<IRequirement> context, IShard shard);
}
