using Kafe.Api.Services;
using Kafe.Data.Capabilities;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kafe.Api.Authorization;

public record AdministratorRequirement : IAuthorizationRequirement;

public class AdministratorHandler : AuthorizationHandler<AdministratorRequirement>
{
    private readonly IUserProvider provider;

    public AdministratorHandler(IUserProvider provider)
    {
        this.provider = provider;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdministratorRequirement requirement)
    {
        if (provider.User is null
            || !provider.User.Capabilities.OfType<Administration>().Any())
        {
            context.Fail(new AuthorizationFailureReason(this, "The user is not an administrator."));
        }

        return Task.CompletedTask;
    }
}
