using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Role;

[ApiVersion("1")]
[Route("role")]
[Authorize]
public class RoleCreationEndpoint(
    RoleService roleService,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
    .WithRequest<RoleCreationDto>
    .WithActionResult<Hrib?>
{
    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.Role])]
    public override async Task<ActionResult<Hrib?>> HandleAsync(
        RoleCreationDto dto,
        CancellationToken cancellationToken = default
    )
    {
        var auth = await authorizationService.AuthorizeAsync(User, dto.OrganizationId, EndpointPolicy.Write);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var role = await roleService.Create(
            RoleInfo.Create(dto.OrganizationId, dto.Name) with
            {
                Description = dto.Description
            },
            cancellationToken
        );

        if (role.HasError)
        {
            return this.KafeErrResult(role);
        }

        return Ok(role.Value.Id);
    }
}
