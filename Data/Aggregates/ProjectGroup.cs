using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using System;

namespace Kafe.Data.Aggregates;

public record ProjectGroup(
    string Id,
    CreationMethod CreationMethod,
    LocalizedString Name,
    LocalizedString? Description = null,
    DateTimeOffset Deadline = default,
    bool IsOpen = false,
    ValidationRules? ValidationRules = null
) : IEntity;

public class ProjectGroupProjection : SingleStreamAggregation<ProjectGroup>
{
    public ProjectGroupProjection()
    {
    }

    public ProjectGroup Create(ProjectGroupCreated e)
    {
        return new ProjectGroup(
            Id: e.ProjectGroupId,
            CreationMethod: e.CreationMethod,
            Name: e.Name
        );
    }

    public ProjectGroup Apply(ProjectGroupInfoChanged e, ProjectGroup g)
    {
        return g with
        {
            Name = e.Name ?? g.Name,
            Description = e.Description ?? g.Description,
            Deadline = e.Deadline ?? g.Deadline
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
