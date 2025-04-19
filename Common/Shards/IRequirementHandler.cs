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

public interface IRequirementHandler<T> : IRequirementHandler
    where T : IRequirement
{
    ValueTask Handle(IRequirementContext<T> context);
}
