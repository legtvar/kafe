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
[Route("organization/{id}")]
public class OrganizationDetailEndpoint(
    OrganizationService organizationService,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<OrganizationDetailDto?>
{
    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.Organization])]
    [ProducesResponseType(typeof(OrganizationDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<OrganizationDetailDto?>> HandleAsync(
        string id,
        CancellationToken ct = default
    )
    {
        var auth = await authorizationService.AuthorizeAsync(User, id, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var data = await organizationService.Load(id, ct);
        if (data.HasError)
        {
            return NotFound();
        }

        return Ok(TransferMaps.ToOrganizationDetailDto(data.Value));
    }
}
