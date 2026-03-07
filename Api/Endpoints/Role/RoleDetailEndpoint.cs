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
[Route("role/{id}")]
public class RoleDetailEndpoint(
    RoleService roleService,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<RoleDetailDto?>
{
    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.Role])]
    [ProducesResponseType(typeof(RoleDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<RoleDetailDto?>> HandleAsync(
        string id,
        CancellationToken ct = default
    )
    {
        var auth = await authorizationService.AuthorizeAsync(User, id, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var roleErr = await roleService.Load(id, ct);
        if (roleErr.HasError)
        {
            return this.KafeErrorResult(roleErr.Diagnostic);
        }

        return Ok(TransferMaps.ToRoleDetailDto(roleErr.Value));
    }
}
