using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Author;

[ApiVersion("1")]
[Route("author")]
public class AuthorCreationEndpoint : EndpointBaseAsync
    .WithRequest<AuthorCreationDto>
    .WithActionResult<Hrib?>
{
    private readonly AuthorService authorService;
    private readonly UserProvider userProvider;

    public AuthorCreationEndpoint(
        AuthorService authorService,
        UserProvider userProvider)
    {
        this.authorService = authorService;
        this.userProvider = userProvider;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.Author })]
    public override async Task<ActionResult<Hrib?>> HandleAsync(
        AuthorCreationDto dto,
        CancellationToken cancellationToken = default)
    {
        var author = await authorService.Create(
            name: dto.Name,
            visibility: dto.Visibility,
            bio: dto.Bio,
            uco: dto.Uco,
            email: dto.Email,
            phone: dto.Phone,
            ownerId: userProvider.Account?.Id,
            token: cancellationToken);
        if (author is null)
        {
            return BadRequest();
        }

        return Ok(author);
    }
}
