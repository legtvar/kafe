using System;
using System.Threading;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Kafe.Api.Endpoints.Account;

[ApiVersion("1")]
[Route("account/external-login")]
public class ExternalAccountLoginEndpoint(
    IOptions<ApiOptions> apiOptions
) : EndpointBaseSync
    .WithRequest<ExternalAccountLoginEndpoint.RequestData>
    .WithActionResult
{
    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Account })]
    public override ActionResult Handle([FromQuery] RequestData request)
    {
        var redirectPath = request.Redirect ?? apiOptions.Value.AccountConfirmPath;

        // Validate redirect URL to prevent open redirect attacks
        if (!string.IsNullOrEmpty(request.Redirect) && !Url.IsLocalUrl(request.Redirect))
        {
            return BadRequest("Invalid redirect URL");
        }

        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = HttpContext.GetFrontEndRedirect(redirectPath)
            },
            OpenIdConnectDefaults.AuthenticationScheme
        );
    }

    public record RequestData
    {
        [FromQuery(Name = "redirect")]
        public string? Redirect { get; set; }
    }
}
