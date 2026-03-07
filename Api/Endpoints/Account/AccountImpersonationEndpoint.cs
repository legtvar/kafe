using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Kafe.Api.Endpoints.Account;

[ApiVersion("1")]
[Route("account/impersonate/{id}")]
[Authorize(EndpointPolicy.Write)]

public class AccountImpersonationEndpoint(
    AccountService accountService,
    UserProvider userProvider
) : EndpointBaseAsync
        .WithRequest<string>
        .WithActionResult
{
    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.Account])]
    public override async Task<ActionResult> HandleAsync(string id, CancellationToken token = default)
    {
        var account = await accountService.Load(id, token);
        if (account.HasError)
        {
            return NotFound();
        }

        await userProvider.SignIn(account.Value);
        return Ok();
    }
}
