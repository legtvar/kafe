using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Data.Services;
using System.Linq;

namespace Kafe.Api.Endpoints.Role;

[ApiVersion("1")]
[Route("role")]
[Authorize]
public class RoleEditEndpoint : EndpointBaseAsync
    .WithRequest<RoleEditDto>
    .WithActionResult<Hrib>
{
    private readonly RoleService roleService;
    private readonly IAuthorizationService authorizationService;

    public RoleEditEndpoint(
        RoleService roleService,
        IAuthorizationService authorizationService)
    {
        this.roleService = roleService;
        this.authorizationService = authorizationService;
    }

    [HttpPatch]
    [SwaggerOperation(Tags = new[] { EndpointArea.Role })]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        RoleEditDto dto,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, dto.Id, EndpointPolicy.Write);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var old = await roleService.Load(dto.Id, cancellationToken);
        if (old is null)
        {
            return NotFound();
        }

        var @new = old with
        {
            Name = dto.Name ?? old.Name,
            Description = dto.Description ?? old.Description,
        };

        var result = await roleService.Edit(@new, cancellationToken);
        if (result.HasErrors)
        {
            return this.KafeErrResult(result);
        }

        return Ok((Hrib)result.Value.Id);
    }
}
