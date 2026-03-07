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

namespace Kafe.Api.Endpoints.Organization;

[ApiVersion("1")]
[Route("organization")]
[Authorize(EndpointPolicy.Write)]
public class OrganizationCreationEndpoint(
    OrganizationService organizationService
) : EndpointBaseAsync
    .WithRequest<OrganizationCreationDto>
    .WithActionResult<Hrib?>
{
    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.Organization])]
    public override async Task<ActionResult<Hrib?>> HandleAsync(
        OrganizationCreationDto dto,
        CancellationToken ct = default
    )
    {
        var organization = await organizationService.Create(OrganizationInfo.Create(dto.Name), ct);

        if (organization.HasError)
        {
            return this.KafeErrResult(organization);
        }

        return Ok(organization.Value.Id);
    }
}
