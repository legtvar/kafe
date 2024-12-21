using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Kafe.Api.Endpoints.Account;

[ApiVersion("1")]
[Route("account/impersonate/{id}")]
[Authorize(EndpointPolicy.Write)]

public class AccountImpersonationEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult
{
    private readonly AccountService accountService;
    private readonly UserProvider userProvider;

    public AccountImpersonationEndpoint(
        AccountService accountService,
        UserProvider userProvider
    )
    {
        this.accountService = accountService;
        this.userProvider = userProvider;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Account })]
    [Tags(EndpointArea.Account)]
    public override async Task<ActionResult> HandleAsync(string id, CancellationToken token = default)
    {
        var account = await accountService.Load((Hrib)id, token);
        if (account is null)
        {
            return NotFound();
        }

        await userProvider.SignIn(account);
        return Ok();
    }
}
