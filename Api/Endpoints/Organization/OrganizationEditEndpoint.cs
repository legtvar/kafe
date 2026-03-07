using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Data.Services;

namespace Kafe.Api.Endpoints.Organization;

[ApiVersion("1")]
[Route("organization")]
[Authorize]
public class OrganizationEditEndpoint(
    OrganizationService organizationService,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
    .WithRequest<OrganizationEditDto>
    .WithActionResult<Hrib>
{
    [HttpPatch]
    [SwaggerOperation(Tags = [EndpointArea.Organization])]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        OrganizationEditDto dto,
        CancellationToken ct = default
    )
    {
        var auth = await authorizationService.AuthorizeAsync(User, dto.Id, EndpointPolicy.Write);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var oldErr = await organizationService.Load(dto.Id, ct);
        if (oldErr.HasError)
        {
            return this.KafeErrResult(oldErr);
        }

        var old = oldErr.Value;
        var @new = old with
        {
            Name = dto.Name ?? old.Name
        };

        var result = await organizationService.Edit(@new, ct);
        if (result.HasError)
        {
            return this.KafeErrResult(result);
        }

        return Ok((Hrib)result.Value.Id);
    }
}
