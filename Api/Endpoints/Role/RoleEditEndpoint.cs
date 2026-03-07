using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Data.Services;

namespace Kafe.Api.Endpoints.Role;

[ApiVersion("1")]
[Route("role")]
[Authorize]
public class RoleEditEndpoint(
    RoleService roleService,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
    .WithRequest<RoleEditDto>
    .WithActionResult<Hrib>
{
    [HttpPatch]
    [SwaggerOperation(Tags = [EndpointArea.Role])]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        RoleEditDto dto,
        CancellationToken cancellationToken = default
    )
    {
        var auth = await authorizationService.AuthorizeAsync(User, dto.Id, EndpointPolicy.Write);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var oldErr = await roleService.Load(dto.Id, cancellationToken);
        if (oldErr.HasError)
        {
            return this.KafeErrorResult(oldErr.Diagnostic);
        }

        var old = oldErr.Value;
        var @new = old with
        {
            Name = dto.Name ?? old.Name,
            Description = dto.Description ?? old.Description,
        };

        var result = await roleService.Edit(@new, cancellationToken);
        if (result.HasError)
        {
            return this.KafeErrResult(result);
        }

        return Ok((Hrib)result.Value.Id);
    }
}
