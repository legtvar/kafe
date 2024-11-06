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

namespace Kafe.Api.Endpoints.Organization;

[ApiVersion("1")]
[Route("organization")]
[Authorize]
public class OrganizationEditEndpoint : EndpointBaseAsync
    .WithRequest<OrganizationEditDto>
    .WithActionResult<Hrib>
{
    private readonly OrganizationService organizationService;
    private readonly IAuthorizationService authorizationService;

    public OrganizationEditEndpoint(
        OrganizationService organizationService,
        IAuthorizationService authorizationService)
    {
        this.organizationService = organizationService;
        this.authorizationService = authorizationService;
    }

    [HttpPatch]
    [SwaggerOperation(Tags = new[] { EndpointArea.Organization })]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        OrganizationEditDto dto,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, dto.Id, EndpointPolicy.Write);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var old = await organizationService.Load(dto.Id, cancellationToken);
        if (old is null)
        {
            return NotFound();
        }

        var @new = old with
        {
            Name = dto.Name ?? old.Name
        };

        var result = await organizationService.Edit(@new, cancellationToken);
        if (result.HasErrors)
        {
            return ValidationProblem(title: result.Errors.FirstOrDefault().Message);
        }

        return Ok(dto.Id);
    }
}
