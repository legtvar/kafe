using System.Threading.Tasks;

namespace Kafe;

public interface IRequirementHandler
{
    ValueTask Handle(RequirementContext context);
}
