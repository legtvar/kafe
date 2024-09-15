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
    public static readonly ProjectGroupInfo Invalid = new(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        OrganizationId: Hrib.InvalidValue,
        Name: LocalizedString.CreateInvariant(Const.InvalidName),
        Description: null
    );

    public ProjectGroupInfo() : this(Invalid)
    {
    }

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
            OrganizationId = organizationId.RawValue
        };
    }
}

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
            OrganizationId: e.OrganizationId ?? Hrib.InvalidValue,
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

    public ProjectGroupInfo Apply(PlaylistMovedToOrganization e, ProjectGroupInfo p)
    {
        return p with
        {
            OrganizationId = e.OrganizationId
        };
    }
}
