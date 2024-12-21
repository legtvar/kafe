using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Options;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Account;

[ApiVersion("1")]
[Route("tmp-account")]
public class TemporaryAccountCreationEndpoint : EndpointBaseAsync
    .WithRequest<TemporaryAccountCreationDto>
    .WithActionResult
{
    private readonly AccountService accountService;
    private readonly IEmailService emailService;
    private readonly IOptions<ApiOptions> apiOptions;
    private readonly ILogger<TemporaryAccountCreationEndpoint> logger;
    private readonly IDataProtector dataProtector;

    public TemporaryAccountCreationEndpoint(
        AccountService accountService,
        IDataProtectionProvider dataProtectionProvider,
        IEmailService emailService,
        IOptions<ApiOptions> apiOptions,
        ILogger<TemporaryAccountCreationEndpoint> logger)
    {
        this.accountService = accountService;
        this.emailService = emailService;
        this.apiOptions = apiOptions;
        this.logger = logger;
        this.dataProtector = dataProtectionProvider.CreateProtector(Const.TemporaryAccountPurpose);
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.Account })]
    [Tags(EndpointArea.Account)]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public override async Task<ActionResult> HandleAsync(
        TemporaryAccountCreationDto dto,
        CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(dto.EmailAddress))
        {
            return BadRequest("Email address is null or white space.");
        }

        dto = dto with { EmailAddress = dto.EmailAddress.Trim() };

        var createRes = await accountService.CreateOrRefreshTemporaryAccount(
            dto.EmailAddress,
            dto.PreferredCulture,
            null,
            token);
        if (createRes.HasErrors)
        {
            return this.KafeErrResult(createRes);
        }

        var account = createRes.Value;

        if (string.IsNullOrEmpty(account.SecurityStamp))
        {
            logger.LogError("Missing security stamp.");
            return StatusCode(500);
        }

        var confirmationToken = EncodeToken(new(account.Id, Const.EmailConfirmationPurpose, account.SecurityStamp));
        var pathString = new PathString(apiOptions.Value.AccountConfirmPath)
            .Add(new PathString("/" + confirmationToken));
        var confirmationUrl = new Uri(new Uri(apiOptions.Value.BaseUrl), pathString);
        var emailSubject = Const.ConfirmationEmailSubject[account!.PreferredCulture]!;
        var emailMessage = string.Format(
            Const.ConfirmationEmailMessageTemplate[account!.PreferredCulture]!,
            confirmationUrl,
            Const.EmailSignOffs[RandomNumberGenerator.GetInt32(0, Const.EmailSignOffs.Length)][account!.PreferredCulture]);
        await emailService.SendEmail(account.EmailAddress, emailSubject, emailMessage, null, token);

        return Ok();
    }

    private string EncodeToken(TemporaryAccountTokenDto dto)
    {
        var token = $"{dto.Purpose}:{dto.AccountId}:{dto.SecurityStamp}";
        var protectedBytes = dataProtector.Protect(Encoding.UTF8.GetBytes(token));
        return WebEncoders.Base64UrlEncode(protectedBytes);
    }
}
