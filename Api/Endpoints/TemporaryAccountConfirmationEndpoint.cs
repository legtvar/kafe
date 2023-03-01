using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Swagger;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints;

[ApiVersion("1")]
[Route("tmp-account/{token}")]
public class TemporaryAccountConfirmationEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult
{
    private readonly IAccountService accounts;

    public TemporaryAccountConfirmationEndpoint(IAccountService accounts)
    {
        this.accounts = accounts;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { SwaggerTags.Account })]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public override async Task<ActionResult> HandleAsync(
        [FromRoute] string token,
        CancellationToken cancellationToken = default)
    {
        await accounts.ConfirmTemporaryAccount(token, cancellationToken);
        return Ok();
    }
}
