using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Api.Services;

namespace Kafe.Api.Endpoints.Playlist;

[ApiVersion("1")]
[Route("playlist/{id}")]
[Authorize]
public class PlaylistDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<PlaylistDetailDto>
{
    private readonly IPlaylistService playlists;

    public PlaylistDetailEndpoint(IPlaylistService playlists)
    {
        this.playlists = playlists;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Playlist })]
    [ProducesResponseType(typeof(PlaylistDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<PlaylistDetailDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var dto = await playlists.Load(id, cancellationToken);
        if (dto is null)
        {
            return NotFound();
        }

        return Ok(dto);
    }
}
