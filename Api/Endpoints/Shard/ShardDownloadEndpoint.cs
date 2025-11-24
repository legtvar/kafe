using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Shard;

[ApiVersion("1")]
[Route("shard-download/{id}")]
[Route("shard-download/{id}/{variant}")]
public class ShardDownloadEndpoint(
    ShardService shardService,
    ArtifactService artifactService,
    ProjectService projectService,
    AccountService accountService,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
    .WithRequest<ShardDownloadEndpoint.RequestData>
    .WithActionResult
{
    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Shard })]
    [Produces(typeof(FileStreamResult))]
    public override async Task<ActionResult> HandleAsync(
        RequestData data,
        CancellationToken cancellationToken = default)
    {
        var detail = await shardService.Load(data.Id, cancellationToken);
        if (detail is null)
        {
            return NotFound();
        }

        var auth = await authorizationService.AuthorizeAsync(User, (Hrib)detail.ArtifactId, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var mediaType = await shardService.GetShardVariantMediaType(data.Id, data.Variant, cancellationToken);
        if (mediaType is null || mediaType.MimeType is null)
        {
            return NotFound();
        }

        var account = await GetProjectOwner((Hrib)detail.ArtifactId, cancellationToken);
        string userPrefix = account != null && account.Uco != null
            ? account.Uco + "_"
            : "";

        var stream = await shardService.OpenStream(data.Id, data.Variant, cancellationToken);
        return File(stream, mediaType.MimeType, $"{userPrefix}{data.Id}_{mediaType.Variant}.{mediaType.FileExtension}", true);
    }

    private async Task<AccountInfo?> GetProjectOwner(Hrib artifactId, CancellationToken cancellationToken)
    {
        var artifact = await artifactService.LoadDetail(artifactId, cancellationToken);
        var containingProjectId = artifact?.ContainingProjectIds.FirstOrDefault();
        if (containingProjectId is null)
            return null;

        var project = await projectService.Load((Hrib)containingProjectId, cancellationToken);
        var ownerId = project?.OwnerId;
        if (ownerId is null)
            return null;

        return await accountService.Load(ownerId, cancellationToken);
    }

    public record RequestData
    {
        [FromRoute]
        public string Id { get; set; } = string.Empty;

        [FromRoute]
        public string? Variant { get; set; }
    }
}
