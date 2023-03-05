using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Account;

[ApiVersion("1")]
[Route("account")]
[Route("account/{id}")]
[Authorize]
public class AccountDetailEndpoint : EndpointBaseAsync
    .WithRequest<string?>
    .WithActionResult<AccountDetailDto?>
{
    private readonly IAccountService accounts;
    private readonly ICurrentAccountProvider currentAccountProvider;

    public AccountDetailEndpoint(IAccountService accounts, ICurrentAccountProvider currentAccountProvider)
    {
        this.accounts = accounts;
        this.currentAccountProvider = currentAccountProvider;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Account })]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<AccountDetailDto?>> HandleAsync(
        string? id,
        CancellationToken cancellationToken = default)
    {
        id ??= currentAccountProvider.User?.Id;

        if (id is null)
        {
            return NotFound("No account ID was provided and the current user doesn't have any. " +
                "This could be a bug.");
        }

        return await accounts.Load((Hrib)id, cancellationToken);
    }
}
