using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Kafe.Api.Endpoints.Account;

[ApiVersion("1")]
[Route("account/logout")]
public class AccountLogoutEndpoint : EndpointBaseSync
    .WithoutRequest
    .WithActionResult
{
    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Account })]
    public override ActionResult Handle()
    {
        return SignOut(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
