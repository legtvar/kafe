using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Options;
using Kafe.Api.Services;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Account;

[ApiVersion("1")]
[Route("tmp-account/{token}")]
public class TemporaryAccountConfirmationEndpoint(
    AccountService accountService,
    UserProvider userProvider,
    IOptions<ApiOptions> apiOptions
) : EndpointBaseAsync
        .WithRequest<TemporaryAccountConfirmationEndpoint.RequestData>
        .WithActionResult
{

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
        if (account.HasError)
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
