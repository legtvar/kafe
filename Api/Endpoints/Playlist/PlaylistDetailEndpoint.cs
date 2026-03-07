using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Data.Services;

namespace Kafe.Api.Endpoints.Playlist;

[ApiVersion("1")]
[Route("playlist/{id}")]
public class PlaylistDetailEndpoint(
    PlaylistService playlistService,
    ArtifactService artifactService,
    IAuthorizationService authorizationService
)
    : EndpointBaseAsync
        .WithRequest<string>
        .WithActionResult<PlaylistDetailDto>
{
    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.Playlist])]
    [ProducesResponseType(typeof(PlaylistDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<PlaylistDetailDto>> HandleAsync(
        string id,
        CancellationToken ct = default
    )
    {
        var auth = await authorizationService.AuthorizeAsync(User, id, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var dataErr = await playlistService.Load(id, ct);
        if (dataErr.HasError)
        {
            return this.KafeErrResult(dataErr);
        }

        var data = dataErr.Value;
        var dto = TransferMaps.ToPlaylistDetailDto(data);

        var artifactsErr = await artifactService.LoadMany([..data.EntryIds.Select(i => (Hrib)i)], ct);
        if (artifactsErr.HasError)
        {
            return this.KafeErrResult(artifactsErr);
        }

        var artifacts = artifactsErr.Value;
        dto = dto with
        {
            Entries = [..artifacts.Select(a => new PlaylistEntryDto(a.Id, a.Name))]
        };

        return Ok(dto);
    }
}
