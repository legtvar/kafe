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
        return p.User is not null && p.User.Capabilities.Contains(new Administration());
    }

    public static bool IsFestivalOrg(this IUserProvider p)
    {
        return p.User is not null && p.User.Capabilities.Contains(new OrganizeFestival());
    }

    public static bool IsProjectReviewer(this IUserProvider p, string role)
    {
        return p.IsAdministrator()
            || (p.User is not null && p.User.Capabilities.Contains(new ProjectReview(role)));
    }

    public static IQueryable<ProjectGroupInfo> WhereCanRead(
        this IQueryable<ProjectGroupInfo> q,
        IUserProvider p)
    {
        if (p.IsAdministrator() || p.IsFestivalOrg())
        {
            return q;
        }

        return q.Where(g => g.IsOpen && g.Deadline > DateTimeOffset.UtcNow);
    }

    public static IQueryable<ProjectInfo> WhereCanRead(
        this IQueryable<ProjectInfo> q,
        IUserProvider userProvider)
    {
        if (userProvider.IsAdministrator() || userProvider.IsFestivalOrg())
        {
            return q;
        }

        var ownProjects = userProvider.GetOwnProjects().Select(i => (string)i).ToImmutableArray();

        return q.Where(p => p.Visibility == Visibility.Public || ownProjects.Contains(p.Id));
    }

    public static IQueryable<AuthorInfo> WhereCanRead(
        this IQueryable<AuthorInfo> q,
        IUserProvider userProvider)
    {
        if (userProvider.IsAdministrator() || userProvider.IsFestivalOrg())
        {
            return q;
        }

        var managedAuthors = userProvider.GetManagedAuthors().Select(i => (string)i).ToImmutableArray();

        return q.Where(p => p.Visibility == Visibility.Public || managedAuthors.Contains(p.Id));
    }

    public static IQueryable<PlaylistInfo> WhereCanRead(
        this IQueryable<PlaylistInfo> q,
        IUserProvider userProvider)
    {
        if (userProvider.IsAdministrator())
        {
            return q;
        }

        return q.Where(p => p.Visibility == Visibility.Public);
    }

    public static bool CanRead(this IUserProvider p, ProjectInfo project)
    {
        if (p.IsFestivalOrg())
        {
            return true;
        }

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

        return CanEdit(p, projectGroup);
    }

    public static bool CanRead(this IUserProvider p, AuthorInfo author)
    {
        if (p.IsAdministrator() || p.IsFestivalOrg())
        {
            return true;
        }

        if (author.Visibility == Visibility.Public)
        {
            return true;
        }

        // TODO: Internal visibility

        return CanEdit(p, author);
    }

    public static bool CanRead(this IUserProvider p, PlaylistInfo playlist)
    {
        if (p.IsAdministrator())
        {
            return true;
        }

        if (playlist.Visibility == Visibility.Public)
        {
            return true;
        }

        // TODO: Internal visibility

        return CanEdit(p, playlist);
    }

    public static bool CanEdit(this IUserProvider p, ProjectInfo project)
    {
        return p.IsAdministrator()
            || (!project.IsLocked && p.IsProjectOwner(project.Id));
    }


    public static bool CanEdit(this IUserProvider p, ProjectGroupInfo projectGroup)
    {
        if (p.IsAdministrator())
        {
            return true;
        }

        // TODO: Implement internal project groups. Will require to implement account grouping as well
        //       (PV110, PV113, etc).

        return projectGroup.IsOpen && projectGroup.Deadline > DateTimeOffset.UtcNow;
    }

    public static bool CanEdit(this IUserProvider p, AuthorInfo author)
    {
        if (p.IsAdministrator())
        {
            return true;
        }

        return p.IsAuthorManager(author.Id);
    }

    public static bool CanEdit(this IUserProvider p, PlaylistInfo playlist)
    {
        return p.IsAdministrator();
    }

    public static ImmutableArray<Hrib> GetManagedAuthors(this IUserProvider p)
    {
        if (p.User is null)
        {
            return ImmutableArray<Hrib>.Empty;
        }

        return p.User.Capabilities.OfType<AuthorManagement>()
            .Select(c => c.AuthorId)
            .ToImmutableArray();
    }

    public static ImmutableArray<Hrib> GetOwnProjects(this IUserProvider p)
    {
        if (p.User is null)
        {
            return ImmutableArray<Hrib>.Empty;
        }

        return p.User.Capabilities.OfType<ProjectOwnership>()
            .Select(c => c.ProjectId)
            .ToImmutableArray();
    }

    public static bool IsAuthorManager(this IUserProvider p, Hrib authorId)
    {
        if (p.User is null)
        {
            return false;
        }

        return p.User.Capabilities.Contains(new AuthorManagement(authorId));
    }

    public static bool IsProjectOwner(this IUserProvider p, Hrib projectId)
    {
        if (p.User is null)
        {
            return false;
        }

        return p.User.Capabilities.Contains(new ProjectOwnership(projectId));
    }

    public static CultureInfo GetPreferredCulture(this IUserProvider p)
    {
        return p.User?.PreferredCulture ?? Const.InvariantCulture;
    }
}
