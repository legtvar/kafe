using Ardalis.ApiEndpoints;
using Asp.Versioning;
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
[Authorize]
public class AuthorEditEndpoint(
    AuthorService authorService,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
    .WithRequest<AuthorEditDto>
    .WithActionResult<Hrib>
{
    [HttpPatch]
    [SwaggerOperation(Tags = new[] { EndpointArea.Author })]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        AuthorEditDto dto,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, dto.Id, EndpointPolicy.Write);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var old = await authorService.Load(dto.Id, cancellationToken);
        if (old is null)
        {
            return NotFound();
        }

        var @new = old with
        {
            Name = dto.Name ?? old.Name,
            GlobalPermissions = dto.GlobalPermissions ?? old.GlobalPermissions,
            Bio = dto.Bio ?? old.Bio,
            Uco = dto.Uco ?? old.Uco,
            Email = dto.Email ?? old.Email,
            Phone = dto.Phone ?? old.Phone
        };

        var result = await authorService.Edit(@new, cancellationToken);
        if (result.HasErrors)
        {
            return this.KafeErrResult(result);
        }

        return Ok((Hrib)result.Value.Id);
    }
}
