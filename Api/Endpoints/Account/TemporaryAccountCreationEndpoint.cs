using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Account;

[ApiVersion("1")]
[Route("tmp-account")]
public class TemporaryAccountCreationEndpoint : EndpointBaseAsync
    .WithRequest<TemporaryAccountCreationDto>
    .WithActionResult
{
    private readonly IAccountService accounts;

    public TemporaryAccountCreationEndpoint(IAccountService accounts)
    {
        this.accounts = accounts;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.Account })]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public override async Task<ActionResult> HandleAsync(
        TemporaryAccountCreationDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.EmailAddress))
        {
            return BadRequest("Email address is null or white space.");
        }

        dto = dto with { EmailAddress = dto.EmailAddress.Trim() };

        await accounts.CreateTemporaryAccount(dto, cancellationToken);
        return Ok();
    }
}
