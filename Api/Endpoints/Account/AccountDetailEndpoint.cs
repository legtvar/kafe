using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Account;

[ApiVersion("1")]
[Route("account")]
[Route("account/{id}")]
public class AccountDetailEndpoint : EndpointBaseAsync
    .WithRequest<string?>
    .WithActionResult<AccountDetailDto?>
{
    private readonly IAccountService accounts;
    private readonly IAuthorizationService authorization;
    private readonly IUserProvider userProvider;

    public AccountDetailEndpoint(
        IAccountService accounts,
        IAuthorizationService authorization,
        IUserProvider userProvider)
    {
        this.accounts = accounts;
        this.authorization = authorization;
        this.userProvider = userProvider;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Account })]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<AccountDetailDto?>> HandleAsync(
        string? id,
        CancellationToken token = default)
    {
        id ??= userProvider.Account?.Id;

        if (id is null)
        {
            return NotFound();
        }

        var account = await accounts.Load2((Hrib)id, token);
        if (account is null)
        {
            return NotFound();
        }

        var authResult = await authorization.AuthorizeAsync(User, account, EndpointPolicy.Read);
        if (!authResult.Succeeded)
        {
            return Unauthorized();
        }

        return TransferMaps.ToAccountDetailDto(account);
    }
}
