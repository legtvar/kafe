using System.Threading.Tasks;

namespace Kafe;

public interface IShardRequirementHandler : IRequirementHandler
{
    ValueTask Handle(IShardRequirementContext<IRequirement> context);
}
