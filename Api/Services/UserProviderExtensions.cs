using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Capabilities;

namespace Kafe.Api.Services;

public static class UserProviderExtensions
{
    public static bool IsAdministrator(this IUserProvider p)
    {
        return p.User is not null && p.User.Capabilities.Contains(new AdministratorCapability());
    }

    public static bool CanReadProject(this IUserProvider p, ProjectInfo project)
    {
        return project.Visibility switch
        {
            Visibility.Public => true,
            // TODO: Implement Visibility.Internal.
            _ => CanEditProject(p, project),
        };
    }

    public static bool CanEditProject(this IUserProvider p, ProjectInfo project)
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
}
