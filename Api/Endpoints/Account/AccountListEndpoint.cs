using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints;

[ApiVersion("1")]
[Route("accounts")]
[Authorize(Policy = EndpointPolicy.Read)]
public class AccountListEndpoint : EndpointBaseAsync
    .WithRequest<AccountListEndpoint.RequestData>
    .WithActionResult<ImmutableArray<AccountListDto>>
{
    private readonly AccountService accounts;

    public AccountListEndpoint(AccountService accounts)
    {
        this.accounts = accounts;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Account })]
    public override async Task<ActionResult<ImmutableArray<AccountListDto>>> HandleAsync(
        [FromQuery] RequestData request,
        CancellationToken cancellationToken = default)
    {
        var filter = new AccountService.AccountFilter();
        if (request.AccessedEntityId is not null)
        {
            filter = filter with {
                Permissions = ImmutableDictionary.CreateRange(new[]{
                    new KeyValuePair<string, Permission>(request.AccessedEntityId, Permission.None)
                })
            };
        }

        var data = await accounts.List(filter, cancellationToken);
        return Ok(data.Select(TransferMaps.ToAccountListDto).ToImmutableArray());
    }
    
    public record RequestData(
        [FromQuery(Name = "entity")] string? AccessedEntityId
    );
}
