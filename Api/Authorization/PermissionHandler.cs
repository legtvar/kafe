using System.Security.Claims;
using System.Threading.Tasks;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Microsoft.AspNetCore.Authorization;

namespace Kafe.Api.Authorization;

public record PermissionRequirement(
    Permission Permission
) : IAuthorizationRequirement;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUserProvider userProvider;

    public PermissionHandler(IUserProvider userProvider)
    {
        this.userProvider = userProvider;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (requirement.Permission == Permission.None)
        {
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is IVisibleEntity visibleEntity
            && userProvider.HasPermission(visibleEntity, requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }
        
        if (context.Resource is IEntity entity
            && userProvider.HasExplicitPermission(entity.Id, requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is Hrib hrib && userProvider.HasExplicitPermission(hrib, requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }

        if (userProvider.HasExplicitPermission(Hrib.System, requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }
    }
}
