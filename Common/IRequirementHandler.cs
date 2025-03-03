using System.Threading.Tasks;

namespace Kafe;

public interface IRequirementHandler
{
    bool CanHandle(IRequirement requirement);

    Task Handle(RequirementContext context);
}
