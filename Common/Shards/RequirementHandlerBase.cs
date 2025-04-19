using System.Threading;
using System.Threading.Tasks;

namespace Kafe;

public abstract class RequirementHandlerBase<T> : IRequirementHandler<T>
    where T : IRequirement
{
    public virtual bool CanHandle(IRequirement requirement)
    {
        return requirement is T;
    }

    public abstract ValueTask Handle(IRequirementContext<T> context);

    ValueTask IRequirementHandler.Handle(IRequirementContext<IRequirement> context)
    {
        return Handle((IRequirementContext<T>)context);
    }
}
