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

namespace Kafe.Api.Endpoints.Organization;

[ApiVersion("1")]
[Route("organization")]
[Authorize(EndpointPolicy.Write)]
public class OrganizationCreationEndpoint : EndpointBaseAsync
    .WithRequest<OrganizationCreationDto>
    .WithActionResult<Hrib?>
{
    private readonly OrganizationService organizationService;
    private readonly UserProvider userProvider;

    public OrganizationCreationEndpoint(
        OrganizationService organizationService,
        UserProvider userProvider)
    {
        this.organizationService = organizationService;
        this.userProvider = userProvider;
    }

    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.Organization])]
    public override async Task<ActionResult<Hrib?>> HandleAsync(
        OrganizationCreationDto dto,
        CancellationToken cancellationToken = default)
    {
        var organization = await organizationService.Create(OrganizationInfo.Create(dto.Name), cancellationToken);

        if (organization.HasErrors)
        {
            return ValidationProblem(title: organization.Errors.First().Message);
        }

        return Ok(organization.Value.Id);
    }
}
