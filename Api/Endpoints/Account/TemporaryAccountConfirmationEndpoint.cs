using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Account;

[ApiVersion("1")]
[Route("tmp-account/{token}")]
public class TemporaryAccountConfirmationEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult
{
    public const string TemporaryAccountRole = "TemporaryAccount";

    private readonly IAccountService accounts;

    public TemporaryAccountConfirmationEndpoint(IAccountService accounts)
    {
        this.accounts = accounts;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Account })]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public override async Task<ActionResult> HandleAsync(
        [FromRoute] string token,
        CancellationToken cancellationToken = default)
    {
        if (!accounts.TryDecodeToken(token, out var tokenDto))
        {
            return Unauthorized();
        }

        await accounts.ConfirmTemporaryAccount(tokenDto, cancellationToken);

        var apiAccount = await accounts.LoadApiAccount(tokenDto.AccountId, cancellationToken);
        if (apiAccount is null)
        {
            return NotFound();
        }

        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = false,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.Add(Const.AuthenticationCookieExpirationTime),
            IsPersistent = true,
        };

        return SignIn(apiAccount.ToPrincipal(CookieAuthenticationDefaults.AuthenticationScheme), authProperties);
    }
}
