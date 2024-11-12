using System;
using System.Threading;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Kafe.Api.Endpoints.Account;

[ApiVersion("1")]
[Route("account/external-login")]
public class ExternalAccountLoginEndpoint : EndpointBaseSync
    .WithRequest<ExternalAccountLoginEndpoint.RequestData>
    .WithActionResult
{
    public const string RedirectUri = "/auth";

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Account })]
    public override ActionResult Handle([FromQuery] RequestData request)
    {
        return Challenge(
            new AuthenticationProperties
            {
                // RedirectUri = new Uri(new Uri(HttpContext.Request.Host.Value), RedirectUri).ToString()
                RedirectUri = request.Redirect ?? "/"
            },
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    public record RequestData
    {
        [FromQuery(Name = "redirect")]
        public string? Redirect { get; set; }
    }
}
