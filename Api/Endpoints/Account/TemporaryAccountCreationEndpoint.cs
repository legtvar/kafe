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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace Kafe.Api.Endpoints.Account;

[ApiVersion("1")]
[Route("tmp-account")]
public class TemporaryAccountCreationEndpoint(
    AccountService accountService,
    IEmailService emailService,
    IOptions<ApiOptions> apiOptions,
    ILogger<TemporaryAccountCreationEndpoint> logger
)
    : EndpointBaseAsync
        .WithRequest<TemporaryAccountCreationDto>
        .WithActionResult
{
    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.Account])]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public override async Task<ActionResult> HandleAsync(
        TemporaryAccountCreationDto dto,
        CancellationToken ct = default
    )
    {
        dto = dto with { EmailAddress = dto.EmailAddress.Trim() };

        var ticket = await accountService.IssueLoginTicket(dto.EmailAddress, dto.PreferredCulture, ct);
        if (ticket.HasErrors)
        {
            return this.KafeErrorResult(ticket.Errors);
        }

        var confirmationToken = accountService.EncodeLoginTicketId(ticket.Value.Id);
        var pathString = new PathString(apiOptions.Value.AccountConfirmPath)
            .Add(new PathString("/" + confirmationToken));
        var confirmationUrl = new Uri(new Uri(apiOptions.Value.BaseUrl), pathString);
        var emailSubject = Const.ConfirmationEmailSubject[ticket.Value.PreferredCulture]!;
        var emailMessage = string.Format(
            Const.ConfirmationEmailMessageTemplate[ticket.Value.PreferredCulture]!,
            confirmationUrl,
            Const.EmailSignOffs[RandomNumberGenerator.GetInt32(0, Const.EmailSignOffs.Length)]
                [ticket.Value.PreferredCulture]
        );
        await emailService.SendEmail(dto.EmailAddress, emailSubject, emailMessage, null, ct);

        return Ok();
    }
}
