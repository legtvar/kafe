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

namespace Kafe.Api.Endpoints.Role;

[ApiVersion("1")]
[Route("role")]
[Authorize]
public class RoleCreationEndpoint : EndpointBaseAsync
    .WithRequest<RoleCreationDto>
    .WithActionResult<Hrib?>
{
    private readonly RoleService roleService;
    private readonly UserProvider userProvider;
    private readonly IAuthorizationService authorizationService;
    

    public RoleCreationEndpoint(
        RoleService roleService,
        UserProvider userProvider,
        IAuthorizationService authorizationService)
    {
        this.roleService = roleService;
        this.userProvider = userProvider;
        this.authorizationService = authorizationService;
    }

    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.Role])]
    public override async Task<ActionResult<Hrib?>> HandleAsync(
        RoleCreationDto dto,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, dto.OrganizationId, EndpointPolicy.Write);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }
        
        var role = await roleService.Create(RoleInfo.Create(dto.OrganizationId, dto.Name) with {
            Description = dto.Description
        }, cancellationToken);

        if (role.HasErrors)
        {
            return role.ToActionResult();
        }

        return Ok(role.Value.Id);
    }
}
