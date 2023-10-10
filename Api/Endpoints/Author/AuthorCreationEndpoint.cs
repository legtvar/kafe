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
    private readonly IUserProvider userProvider;

    public AuthorCreationEndpoint(
        IAuthorService authors,
        IUserProvider userProvider)
    {
        this.authors = authors;
        this.userProvider = userProvider;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.Author })]
    public override async Task<ActionResult<Hrib?>> HandleAsync(
        AuthorCreationDto dto,
        CancellationToken cancellationToken = default)
    {
        var author = await authors.Create(dto, userProvider.Account?.Id, cancellationToken);
        if (author is null)
        {
            return BadRequest();
        }

        return Ok(author);
    }
}
