using System.Collections.Generic;

namespace Kafe;

public sealed class RequirementContext
{
    private readonly List<RequirementMessage> messages = [];

    public IReadOnlyList<RequirementMessage> Messages { get; }

    public IRequirement Requirement { get; }

    public KafeObject Object { get; set; }

    public RequirementContext(IRequirement requirement, KafeObject @object)
    {
        Requirement = requirement;
        Object = @object;
        Messages = messages.AsReadOnly();
    }

    public RequirementContext Report(RequirementMessage message)
    {
        messages.Add(message);
        return this;
    }
}
