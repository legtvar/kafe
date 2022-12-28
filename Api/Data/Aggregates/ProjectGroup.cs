using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record ProjectGroup(
    string Id,
    CreationMethod CreationMethod,
    string? Name = null,
    string? Description = null,
    string? EnglishName = null,
    string? EnglishDescription = null,
    DateTimeOffset Deadline = default,
    bool IsOpen = false,
    ValidationRules? ValidationRules = null
);

public class ProjectGroupProjection : SingleStreamAggregation<ProjectGroup>
{
    public ProjectGroupProjection()
    {
    }

    public ProjectGroup Create(IEvent<ProjectGroupCreated> e)
    {
        return new ProjectGroup(
            Id: e.StreamKey!,
            CreationMethod: e.Data.CreationMethod
        );
    }

    public ProjectGroup Apply(ProjectGroupInfoChanged e, ProjectGroup g)
    {
        return g with
        {
            Name = e.Name,
            Description = e.Description,
            EnglishName = e.EnglishName,
            EnglishDescription = e.EnglishDescription,
            Deadline = e.Deadline
        };
    }

    public ProjectGroup Apply(ProjectGroupOpened e, ProjectGroup g)
    {
        return g with { IsOpen = true };
    }

    public ProjectGroup Apply(ProjectGroupClosed e, ProjectGroup g)
    {
        return g with { IsOpen = false };
    }

    public ProjectGroup Apply(ProjectGroupValidationRulesChanged e, ProjectGroup g)
    {
        return g with { ValidationRules = e.ValidationRules };
    }
}
