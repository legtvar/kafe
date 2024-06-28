using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
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
    private readonly AuthorService authorService;
    private readonly AccountService accountService;
    private readonly UserProvider userProvider;

    public AuthorCreationEndpoint(
        AuthorService authorService,
        AccountService accountService,
        UserProvider userProvider)
    {
        this.authorService = authorService;
        this.accountService = accountService;
        this.userProvider = userProvider;
    }

    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.Author])]
    public override async Task<ActionResult<Hrib?>> HandleAsync(
        AuthorCreationDto dto,
        CancellationToken cancellationToken = default)
    {
        var author = await authorService.Create(AuthorInfo.Invalid with
        {
            Name = dto.Name,
            Bio = dto.Bio,
            Uco = dto.Uco,
            Email = dto.Email,
            Phone = dto.Phone
        }, cancellationToken);

        if (author.HasErrors)
        {
            return ValidationProblem(title: author.Errors.First().Message);
        }

        return Ok(author.Value.Id);
    }
}
