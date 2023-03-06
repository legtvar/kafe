using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Capabilities;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Kafe.Api.Services;

public static class UserProviderExtensions
{
    public static bool IsAdministrator(this IUserProvider p)
    {
        return p.User is not null && p.User.Capabilities.Contains(new AdministratorCapability());
    }

    public static IQueryable<ProjectGroupInfo> WhereCanRead(
        this IQueryable<ProjectGroupInfo> q,
        IUserProvider p)
    {
        if (p.IsAdministrator())
        {
            return q;
        }

        return q.Where(g => g.IsOpen && g.Deadline < DateTimeOffset.UtcNow);
    }

    public static IQueryable<ProjectInfo> WhereCanRead(
        this IQueryable<ProjectInfo> q,
        IUserProvider userProvider)
    {
        if (userProvider.IsAdministrator())
        {
            return q;
        }

        var ownProjects = userProvider.GetOwnProjects();

        return q.Where(p => p.Visibility == Visibility.Public || ownProjects.Contains(p.Id));
    }

    public static bool CanRead(this IUserProvider p, ProjectInfo project)
    {
        return project.Visibility switch
        {
            Visibility.Public => true,
            // TODO: Implement Visibility.Internal.
            _ => CanEdit(p, project),
        };
    }

    public static bool CanRead(this IUserProvider p, ProjectGroupInfo projectGroup)
    {
        if (p.IsAdministrator())
        {
            return true;
        }

        return projectGroup.IsOpen && projectGroup.Deadline < DateTimeOffset.UtcNow;
    }

    public static bool CanEdit(this IUserProvider p, ProjectInfo project)
    {
        if (p.IsAdministrator())
        {
            return true;
        }

        return project.Visibility switch
        {
            Visibility.Private => p.User?.Capabilities.Contains(new ProjectOwnerCapability(project.Id)) == true,
            _ => false
        };
    }

    public static bool CanReadProjectGroup(this IUserProvider p, ProjectGroupInfo projectGroup)
    {
        if (p.IsAdministrator())
        {
            return true;
        }

        // TODO: Implement internal project groups. Will require to implement account grouping as well
        //       (PV110, PV113, etc).

        return projectGroup.IsOpen;
    }

    public static ImmutableHashSet<Hrib> GetOwnProjects(this IUserProvider p)
    {
        if (p.User is null)
        {
            return ImmutableHashSet<Hrib>.Empty;
        }

        return p.User.Capabilities.OfType<ProjectOwnerCapability>()
            .Select(c => c.ProjectId)
            .ToImmutableHashSet();
    }

    public static CultureInfo GetPreferredCulture(this IUserProvider p)
    {
        return new CultureInfo(p.User?.PreferredCulture ?? Const.InvariantCultureCode);
    }
}
