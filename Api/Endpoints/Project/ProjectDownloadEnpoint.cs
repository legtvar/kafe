using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Project;

[ApiVersion("1")]
[Route("project-download/{id}")]
public class ProjectDownloadEndpoint(
    ProjectService projectService,
    ShardService shardService,
    ArtifactService artifactService,
    AccountService accountService,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
    .WithRequest<ProjectDownloadEndpoint.RequestData>
    .WithActionResult
{
    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    [Produces(typeof(FileStreamResult))]
    public override async Task<ActionResult> HandleAsync(
        RequestData data,
        CancellationToken cancellationToken = default)
    {
        var project = await projectService.Load(data.Id, cancellationToken);
        if (project is null)
            return NotFound();

        var auth = await authorizationService.AuthorizeAsync(User, project.Id, EndpointPolicy.Review);
        if (!auth.Succeeded)
            return Unauthorized();

        var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            var artifactDetails = await artifactService.LoadDetailMany(
                project.Artifacts.Select(a => (Hrib)a.Id).Distinct(),
                cancellationToken);
            foreach (var artifact in artifactDetails)
            {
                foreach (var shard in artifact.Shards)
                {
                    var shardDetail = await shardService.Load(shard.ShardId, cancellationToken);
                    if (shardDetail is null) continue;

                    foreach (var variant in shard.Variants)
                    {
                        var mediaType = await shardService.GetShardVariantMediaType(shard.ShardId, variant, cancellationToken);
                        if (mediaType?.FileExtension == null) continue;

                        var shardStream = await shardService.OpenStream(shard.ShardId, variant, cancellationToken);

                        var entryName = $"{shard.ShardId}/{variant}{mediaType.FileExtension}";
                        var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
                        using var entryStream = entry.Open();
                        await shardStream.CopyToAsync(entryStream, cancellationToken);
                    }
                }
            }
        }

        var fileName = $"{project.Name.GetValueOrDefault("iv", project.Id)}.zip";
        if (project.OwnerId != null)
        {
            var owner = await accountService.Load(project.OwnerId, cancellationToken);
            if (owner != null)
            {
                fileName = owner.Name != null ? $"{owner.Name}_{fileName}" : fileName;
                fileName = owner.Uco != null ? $"{owner.Uco}_{fileName}" : fileName;
            }
        }
        zipStream.Position = 0;
        return File(zipStream, "application/zip", fileName, true);
    }

    public record RequestData
    {
        [FromRoute]
        public string Id { get; set; } = string.Empty;
    }
}
