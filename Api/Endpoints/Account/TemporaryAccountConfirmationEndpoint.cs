using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Options;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Account;

[ApiVersion("1")]
[Route("tmp-account/{token}")]
public class TemporaryAccountConfirmationEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult
{
    private readonly AccountService accountService;
    private readonly UserProvider userProvider;
    private readonly IDataProtector dataProtector;


    public TemporaryAccountConfirmationEndpoint(
        AccountService accountService,
        UserProvider userProvider,
        IDataProtectionProvider dataProtectionProvider
    )
    {
        this.accountService = accountService;
        this.userProvider = userProvider;
        this.dataProtector = dataProtectionProvider.CreateProtector(Const.TemporaryAccountPurpose);
    }

    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.Account])]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult> HandleAsync(
        [FromRoute] string token,
        CancellationToken ct = default
    )
    {
        if (!TryDecodeToken(token, out var loginTicketId))
        {
            return Unauthorized();
        }

        var account = await accountService.PunchTicket(
            loginTicketId.Value,
            ct
        );
        if (account.HasErrors)
        {
            // NB: we disregard any specific information about the errors for security reasons
            return Unauthorized();
        }

        await userProvider.SignIn(account.Value);

        return Ok();
    }

    private bool TryDecodeToken(string encodedToken, [NotNullWhen(true)] out Guid? loginTicketId)
    {
        try
        {
            var unprotectedBytes = dataProtector.Unprotect(WebEncoders.Base64UrlDecode(encodedToken));
            loginTicketId = new Guid(unprotectedBytes);
            return true;
        }
        catch (Exception)
        {
            loginTicketId = null;
            return false;
        }
    }
}
