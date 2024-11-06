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
public class RoleDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<RoleDetailDto?>
{
    private readonly RoleService roleService;
    private readonly IAuthorizationService authorizationService;

    public RoleDetailEndpoint(
        RoleService roleService,
        IAuthorizationService authorizationService)
    {
        this.roleService = roleService;
        this.authorizationService = authorizationService;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Role })]
    [ProducesResponseType(typeof(RoleDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<RoleDetailDto?>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, id, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var data = await roleService.Load(id, cancellationToken);
        if (data is null)
        {
            return NotFound();
        }

        return Ok(TransferMaps.ToRoleDetailDto(data));
    }
}
