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
    public static readonly TimeSpan CookieExpirationTime = new(30, 0, 0, 0);

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
        var account = await accounts.ConfirmTemporaryAccount(token, cancellationToken);
        if (account is null)
        {
            return BadRequest();
        }

        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, account.EmailAddress),
            new Claim(ClaimTypes.NameIdentifier, account.Id),
            new Claim(ClaimTypes.Role, TemporaryAccountRole)
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = false,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.Add(CookieExpirationTime),
            IsPersistent = true
        };

        return SignIn(new ClaimsPrincipal(claimsIdentity), authProperties);
    }
}
