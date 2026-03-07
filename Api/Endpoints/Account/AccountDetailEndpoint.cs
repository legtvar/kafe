using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Account;

[ApiVersion("1")]
[Route("account")]
[Route("account/{id}")]
[Authorize]
public class AccountDetailEndpoint(
    AccountService accounts,
    IAuthorizationService authorization,
    UserProvider userProvider
) : EndpointBaseAsync
        .WithRequest<string?>
        .WithActionResult<AccountDetailDto?>
{
    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.Account])]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<AccountDetailDto?>> HandleAsync(
        string? id,
        CancellationToken token = default)
    {
        id ??= userProvider.AccountId.ToString();

        if (id == Hrib.EmptyValue)
        {
            return NotFound();
        }

        var account = await accounts.Load(id, token);
        if (account.HasError)
        {
            return NotFound();
        }

        var authResult = await authorization.AuthorizeAsync(User, account, EndpointPolicy.Read);
        if (!authResult.Succeeded)
        {
            return Unauthorized();
        }

        return TransferMaps.ToAccountDetailDto(account.Value);
    }
}
