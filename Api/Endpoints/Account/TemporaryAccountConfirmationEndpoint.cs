using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Options;
using Kafe.Api.Services;
using Kafe.Data.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Account;

[ApiVersion("1")]
[Route("tmp-account/{token}")]
public class TemporaryAccountConfirmationEndpoint : EndpointBaseAsync
    .WithRequest<TemporaryAccountConfirmationEndpoint.RequestData>
    .WithActionResult
{
    private readonly AccountService accountService;
    private readonly UserProvider userProvider;
    private readonly IOptions<ApiOptions> apiOptions;

    public TemporaryAccountConfirmationEndpoint(
        AccountService accountService,
        UserProvider userProvider,
        IOptions<ApiOptions> apiOptions
    )
    {
        this.accountService = accountService;
        this.userProvider = userProvider;
        this.apiOptions = apiOptions;
    }

    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.Account])]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult> HandleAsync(
        RequestData request,
        CancellationToken ct = default
    )
    {
        var ticketId = accountService.DecodeLoginTicketId(request.Token);
        if (ticketId is null)
        {
            return Unauthorized();
        }

        var account = await accountService.PunchTicket(
            ticketId.Value,
            ct: ct
        );
        if (account.HasErrors)
        {
            // NB: we disregard any specific information about the errors for security reasons
            return Unauthorized();
        }

        await userProvider.SignIn(account.Value);

        return Redirect(
            HttpContext.GetFrontEndRedirect(
                request.Redirect ?? apiOptions.Value.AccountConfirmRedirectPath
            )
        );
    }

    public record RequestData
    {
        [FromRoute]
        public string Token { get; set; } = string.Empty;

        [FromQuery(Name = "redirect")]
        public string? Redirect { get; set; }
    }
}
