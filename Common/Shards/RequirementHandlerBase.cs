using System.Threading.Tasks;

namespace Kafe;

public abstract class RequirementHandlerBase<T> : IRequirementHandler where T : IRequirement
{
    public virtual bool CanHandle(IRequirement requirement)
    {
        return requirement is T;
    }

    public abstract ValueTask Handle(RequirementContext context);
}
