using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Author;

[ApiVersion("1")]
[Route("author")]
[Authorize]
public class AuthorCreationEndpoint : EndpointBaseAsync
    .WithRequest<AuthorCreationDto>
    .WithActionResult<Hrib?>
{
    private readonly IAuthorService authors;

    public AuthorCreationEndpoint(IAuthorService authors)
    {
        this.authors = authors;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.Author })]
    public override async Task<ActionResult<Hrib?>> HandleAsync(
        AuthorCreationDto dto,
        CancellationToken cancellationToken = default)
    {
        var id = await authors.Create(dto, cancellationToken);
        if (id is null)
        {
            return BadRequest();
        }

        return Ok(id);
    }
}
