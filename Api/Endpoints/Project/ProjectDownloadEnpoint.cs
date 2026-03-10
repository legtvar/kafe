using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Core;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Immutable;
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
    IAuthorizationService authorizationService,
    FileExtensionMimeMap mimeMap
) : EndpointBaseAsync
    .WithRequest<ProjectDownloadEndpoint.RequestData>
    .WithActionResult
{
    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    [Produces(typeof(FileStreamResult))]
    public override async Task<ActionResult> HandleAsync(
        RequestData data,
        CancellationToken ct = default
    )
    {
        var projectErr = await projectService.Load(data.Id, ct);
        if (projectErr.HasError)
        {
            return this.KafeErrorResult(projectErr.Diagnostic);
        }

        var project = projectErr.Value;
        var auth = await authorizationService.AuthorizeAsync(User, project.Id, EndpointPolicy.Review);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        if (project.ArtifactId is null)
        {
            return BadRequest("This project has no inner artifact and therefore cannot be downloaded.");
        }

        var artifactErr = await artifactService.Load(project.ArtifactId, ct);
        if (artifactErr.HasError)
        {
            return this.KafeErrorResult(artifactErr.Diagnostic);
        }

        var artifact = artifactErr.Value;

        var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            var shardProps = artifact?.Properties.Where(p => p.Value.Value is ShardReference).ToImmutableArray() ?? [];
            var shardsErr = await shardService.LoadMany(
                [.. shardProps.Select(p => ((ShardReference)p.Value.Value).ShardId)],
                ct
            );
            if (shardsErr.HasError)
            {
                return this.KafeErrorResult(shardsErr.Diagnostic);
            }

            foreach (var shard in shardsErr.Value)
            {
                var shardStreamErr = await shardService.OpenStream(shard.Id, Const.OriginalShardVariant, ct);
                if (shardStreamErr.HasError)
                {
                    return this.KafeErrorResult(shardStreamErr.Diagnostic);
                }

                var shardStream = shardStreamErr.Value;
                var extension = mimeMap.GetFirstFileExtensionFor(shard.MimeType);
                var entryName = $"{shard.Id}{extension}";
                var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
                using var entryStream = entry.Open();
                await shardStream.CopyToAsync(entryStream, ct);
            }
        }

        var fileName = $"{artifact?.Name["iv"] ?? project.Id}.zip";
        if (project.OwnerId != null)
        {
            var ownerErr = await accountService.Load(project.OwnerId, ct);
            if (ownerErr.HasError)
            {
                return this.KafeErrorResult(ownerErr.Diagnostic);
            }

            var owner = ownerErr.Value;
            fileName = owner.Name != null ? $"{owner.Name}_{fileName}" : fileName;
            fileName = owner.Uco != null ? $"{owner.Uco}_{fileName}" : fileName;
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
