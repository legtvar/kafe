using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints;

[ApiVersion("1")]
[Route("accounts")]
[Authorize(Policy = EndpointPolicy.Read)]
public class AccountListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<ImmutableArray<AccountListDto>>
{
    private readonly IAccountService accounts;

    public AccountListEndpoint(IAccountService accounts)
    {
        this.accounts = accounts;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Account })]
    public override async Task<ActionResult<ImmutableArray<AccountListDto>>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return Ok(await accounts.List(cancellationToken));
    }
}
