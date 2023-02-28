using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Swagger;
using Kafe.Api.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints;

[ApiVersion("1")]
[Route("artifact")]
[Authorize]
public class ArtifactCreationEndpoint : EndpointBaseAsync
    .WithRequest<ArtifactCreationDto>
    .WithActionResult<Hrib>
{
    private readonly IArtifactService artifacts;

    public ArtifactCreationEndpoint(IArtifactService artifacts)
    {
        this.artifacts = artifacts;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { SwaggerTags.Artifact })]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ArtifactCreationDto request,
        CancellationToken cancellationToken = default)
    {
        var hrib = await artifacts.Create(request, cancellationToken);
        return Ok(hrib);
    }
}
