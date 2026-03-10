using System;
using System.Threading.Tasks;

namespace Kafe;

public abstract class RequirementHandlerBase<T> : IRequirementHandler
    where T : IRequirement
{
    public virtual bool CanHandle(IRequirement requirement)
    {
        return requirement is T;
    }

    public abstract ValueTask Handle(IRequirementContext<T> context);

    ValueTask IRequirementHandler.Handle(IRequirementContext<IRequirement> context)
    {
        if (context is not IRequirementContext<T> concreteContext)
        {
            if (context.Requirement is not T concreteRequirement)
            {
                throw new InvalidOperationException(
                    "This requirement handler cannot be given a context for "
                    + $@"a requirement of type '{context.Requirement.GetType()}'."
                );
            }

            concreteContext = new RequirementContext<T>(
                requirement: concreteRequirement,
                target: context.RawTarget,
                serviceProvider: context.ServiceProvider,
                cancellationToken: context.CancellationToken
            )
            {
                Diagnostics = context.Diagnostics
            };
        }

        return Handle(concreteContext);
    }
}
