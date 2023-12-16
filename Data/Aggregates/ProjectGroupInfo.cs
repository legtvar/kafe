using Kafe.Data.Events;
using Marten.Events.Aggregation;
using System;
using System.Collections.Immutable;

namespace Kafe.Data.Aggregates;

public record ProjectGroupInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    [LocalizedString] ImmutableDictionary<string, string> Name,
    [LocalizedString] ImmutableDictionary<string, string>? Description = null,
    DateTimeOffset Deadline = default,
    bool IsOpen = false,
    Permission GlobalPermissions = Permission.None
) : IVisibleEntity;

public class ProjectGroupInfoProjection : SingleStreamProjection<ProjectGroupInfo>
{
    public ProjectGroupInfoProjection()
    {
    }

    public static ProjectGroupInfo Create(ProjectGroupCreated e)
    {
        return new ProjectGroupInfo(
            Id: e.ProjectGroupId,
            CreationMethod: e.CreationMethod,
            Name: e.Name
        );
    }

    public ProjectGroupInfo Apply(ProjectGroupInfoChanged e, ProjectGroupInfo g)
    {
        return g with
        {
            Name = e.Name ?? g.Name,
            Description = e.Description ?? g.Description,
            Deadline = e.Deadline ?? g.Deadline,
            GlobalPermissions = e.GlobalPermissions ?? g.GlobalPermissions
        };
    }

    public ProjectGroupInfo Apply(ProjectGroupOpened e, ProjectGroupInfo g)
    {
        return g with { IsOpen = true };
    }

    public ProjectGroupInfo Apply(ProjectGroupClosed e, ProjectGroupInfo g)
    {
        return g with { IsOpen = false };
    }

    public ProjectGroupInfo Apply(GlobalPermissionsChanged e, ProjectGroupInfo a)
    {
        return a with
        {
            GlobalPermissions = e.GlobalPermissions
        };
    }
}
