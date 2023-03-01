using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Swagger;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints;

[ApiVersion("1")]
[Route("project")]
[Authorize]
public class ProjectCreationEndpoint : EndpointBaseAsync
    .WithRequest<ProjectCreationDto>
    .WithActionResult<Hrib>
{
    [HttpPost]
    [SwaggerOperation(Tags = new[] { SwaggerTags.Project })]
    public override Task<ActionResult<Hrib>> HandleAsync(
        ProjectCreationDto request,
        CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}
