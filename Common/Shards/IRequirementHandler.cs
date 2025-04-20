using System.Threading.Tasks;

namespace Kafe;

public interface IRequirementHandler
{
    bool CanHandle(IRequirement requirement)
    {
        return true;
    }

    ValueTask Handle(IRequirementContext<IRequirement> context);
}
