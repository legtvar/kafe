using Kafe.Data.Events;
using Marten.Events.Aggregation;
using Marten.Events.CodeGeneration;
using System;
using System.Collections.Immutable;

namespace Kafe.Data.Aggregates;

public record ProjectGroupInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    [Hrib] string OrganizationId,
    [LocalizedString] ImmutableDictionary<string, string> Name,
    [LocalizedString] ImmutableDictionary<string, string>? Description = null,
    DateTimeOffset Deadline = default,
    bool IsOpen = false,
    Permission GlobalPermissions = Permission.None
) : IVisibleEntity
{
    public ProjectGroupInfo() : this(Invalid)
    {
    }

    public static readonly ProjectGroupInfo Invalid = new(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        OrganizationId: Hrib.InvalidValue,
        Name: LocalizedString.CreateInvariant(Const.InvalidName),
        Description: null
    );

    /// <summary>
    /// Creates a bare-bones but valid <see cref="ProjectGroupInfo"/>.
    /// </summary>
    [MartenIgnore]
    public static ProjectGroupInfo Create(Hrib organizationId, LocalizedString name)
    {
        return new ProjectGroupInfo
        {
            Id = Hrib.EmptyValue,
            Name = name,
            OrganizationId = organizationId.Value
        };
    }
}

public class ProjectGroupInfoProjection : SingleStreamProjection<ProjectGroupInfo>
{
    public ProjectGroupInfoProjection()
    {
    }

    public static ProjectGroupInfo Create(ProjectGroupEstablished e)
    {
        return new ProjectGroupInfo(
            Id: e.ProjectGroupId,
            CreationMethod: e.CreationMethod,
            OrganizationId: e.OrganizationId,
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

    public ProjectGroupInfo Apply(ProjectGroupGlobalPermissionsChanged e, ProjectGroupInfo a)
    {
        return a with
        {
            GlobalPermissions = e.GlobalPermissions
        };
    }
}
