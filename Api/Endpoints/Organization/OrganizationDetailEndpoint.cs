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
public class OrganizationDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<OrganizationDetailDto?>
{
    private readonly OrganizationService organizationService;
    private readonly IAuthorizationService authorizationService;

    public OrganizationDetailEndpoint(
        OrganizationService organizationService,
        IAuthorizationService authorizationService)
    {
        this.organizationService = organizationService;
        this.authorizationService = authorizationService;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Organization })]
    [ProducesResponseType(typeof(OrganizationDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<OrganizationDetailDto?>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, id, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var data = await organizationService.Load(id, cancellationToken);
        if (data is null)
        {
            return NotFound();
        }

        return Ok(TransferMaps.ToOrganizationDetailDto(data));
    }
}
