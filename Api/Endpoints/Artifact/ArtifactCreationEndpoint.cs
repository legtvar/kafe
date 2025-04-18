using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Marten.Linq.SoftDeletes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Artifact;

[ApiVersion("1")]
[Route("artifact")]
[Authorize]
public class ArtifactCreationEndpoint : EndpointBaseAsync
    .WithRequest<ArtifactCreationDto>
    .WithActionResult<Hrib>
{
    private readonly ArtifactService artifacts;
    private readonly ProjectService projectService;
    private readonly IAuthorizationService authorization;

    public ArtifactCreationEndpoint(
        ArtifactService artifacts,
        ProjectService projectService,
        IAuthorizationService authorization)
    {
        this.artifacts = artifacts;
        this.projectService = projectService;
        this.authorization = authorization;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.Artifact })]
    [ProducesResponseType(typeof(Hrib), 200)]
    [ProducesResponseType(400)]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ArtifactCreationDto request,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorization.AuthorizeAsync(User, request.ContainingProject, EndpointPolicy.Append);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var createResult = await artifacts.Create(ArtifactInfo.Create(name: request.Name) with
        {
            AddedOn = request.AddedOn ?? default,
            CreationMethod = CreationMethod.Api
        },
            cancellationToken);

        if (createResult.Diagnostic is not null)
        {
            return this.KafeErrResult(createResult);
        }

        if (request.ContainingProject is not null)
        {
            var addArtifactsResult = await projectService.AddArtifacts(
                request.ContainingProject,
                [(createResult.Value.Id, request.BlueprintSlot)]);
            if (addArtifactsResult.Diagnostic is not null)
            {
                return this.KafeErrResult(addArtifactsResult);
            }
        }

        return Ok((Hrib)createResult.Value.Id);
    }
}
