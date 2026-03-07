using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Data.Services;

namespace Kafe.Api.Endpoints.Author;

[ApiVersion("1")]
[Route("author/{id}")]
public class AuthorDetailEndpoint(
    AuthorService authorService,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
        .WithRequest<string>
        .WithActionResult<AuthorDetailDto?>
{
    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.Author])]
    [ProducesResponseType(typeof(AuthorDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<AuthorDetailDto?>> HandleAsync(
        string id,
        CancellationToken ct = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, id, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var authorErr = await authorService.Load(id, ct);
        if (authorErr.HasError)
        {
            return NotFound();
        }

        return Ok(TransferMaps.ToAuthorDetailDto(authorErr.Value));
    }
}
