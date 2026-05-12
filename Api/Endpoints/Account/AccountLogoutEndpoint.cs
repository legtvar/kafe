using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Options;
using Kafe.Api.Services;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Kafe.Api.Endpoints.Account;

[ApiVersion("1")]
[Route("account/logout")]
public class AccountLogoutEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult
{
    private readonly UserProvider userProvider;
    private readonly AccountService accountService;
    private readonly IOptions<ApiOptions> apiOptions;

    public AccountLogoutEndpoint(
        UserProvider userProvider,
        AccountService accountService,
        IOptions<ApiOptions> apiOptions
    )
    {
        this.userProvider = userProvider;
        this.accountService = accountService;
        this.apiOptions = apiOptions;
    }

    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.Account])]
    public override async Task<ActionResult> HandleAsync(CancellationToken token = default)
    {
        var impersonatedAccount = userProvider.GetImpersonatedAccount();
        if (impersonatedAccount is not null)
        {
            await userProvider.Unimpersonate();
            return Redirect(
                HttpContext.GetFrontEndRedirect(
                    apiOptions.Value.AccountConfirmRedirectPath
                )
            );
        }

        return SignOut(
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme
        );
    }
}
