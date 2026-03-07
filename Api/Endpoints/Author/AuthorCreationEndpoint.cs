using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Author;

[ApiVersion("1")]
[Route("author")]
[Authorize]
public class AuthorCreationEndpoint(
    AuthorService authorService,
    UserProvider userProvider
) : EndpointBaseAsync
        .WithRequest<AuthorCreationDto>
        .WithActionResult<Hrib?>
{
    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.Author])]
    public override async Task<ActionResult<Hrib?>> HandleAsync(
        AuthorCreationDto dto,
        CancellationToken cancellationToken = default)
    {
        var author = await authorService.Create(
            @new: AuthorInfo.Create(dto.Name) with
            {
                Bio = dto.Bio,
                Uco = dto.Uco,
                Email = dto.Email,
                Phone = dto.Phone
            },
            ownerId: userProvider.AccountId,
            token: cancellationToken);

        if (author.HasError)
        {
            return this.KafeErrResult(author);
        }

        return Ok(author.Value.Id);
    }
}
