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
        IDataProtectionProvider dataProtectionProvider)
    {
        this.accountService = accountService;
        this.userProvider = userProvider;
        this.dataProtector = dataProtectionProvider.CreateProtector(Const.TemporaryAccountPurpose);
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Account })]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult> HandleAsync(
        [FromRoute] string token,
        CancellationToken cancellationToken = default)
    {
        if (!TryDecodeToken(token, out var tokenDto))
        {
            return Unauthorized();
        }

        if (tokenDto.Purpose != Const.EmailConfirmationPurpose)
        {
            return Unauthorized();
        }

        if (!await accountService.TryConfirmTemporaryAccount(
            tokenDto.AccountId,
            tokenDto.SecurityStamp,
            cancellationToken))
        {
            return Unauthorized();
        }

        var account = await accountService.Load(tokenDto.AccountId, cancellationToken);
        if (account is null)
        {
            return NotFound();
        }

        await userProvider.SignIn(account);

        return Ok();
    }

    private bool TryDecodeToken(string encodedToken, [NotNullWhen(true)] out TemporaryAccountTokenDto? dto)
    {
        try
        {
            var unprotectedBytes = dataProtector.Unprotect(WebEncoders.Base64UrlDecode(encodedToken));
            var token = Encoding.UTF8.GetString(unprotectedBytes);
            var fields = token.Split(':', 3);
            if (fields.Length != 3)
            {
                dto = null;
                return false;
            }

            dto = new(Purpose: fields[0], AccountId: fields[1], SecurityStamp: fields[2]);
            return true;
        }
        catch (Exception)
        {
            dto = null;
            return false;
        }
    }
}
