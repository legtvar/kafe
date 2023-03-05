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
[Authorize]
public class CurrentAccountDetailEndpoint : EndpointBaseAsync
    .WithRequest<string?>
    .WithActionResult<AccountDetailDto?>
{
    private readonly IAccountService accounts;

    public CurrentAccountDetailEndpoint(IAccountService accounts)
    {
        this.accounts = accounts;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.Account })]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public override async Task<ActionResult<AccountDetailDto?>> HandleAsync(
        [FromRoute] string? id,
        CancellationToken cancellationToken = default)
    {
        id ??= User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (id is null)
        {
            throw new ArgumentException("No account ID was provided and the current user doesn't have any. " +
                "This could be a bug.");
        }

        return await accounts.Load(id, cancellationToken);
    }
}
