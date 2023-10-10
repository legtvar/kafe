using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Api.Services;
using Kafe.Data.Services;

namespace Kafe.Api.Endpoints.Playlist;

[ApiVersion("1")]
[Route("playlist/{id}")]
public class PlaylistDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<PlaylistDetailDto>
{
    private readonly PlaylistService playlistService;
    private readonly IAuthorizationService authorizationService;

    public PlaylistDetailEndpoint(
        PlaylistService playlistService,
        IAuthorizationService authorizationService)
    {
        this.playlistService = playlistService;
        this.authorizationService = authorizationService;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Playlist })]
    [ProducesResponseType(typeof(PlaylistDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<PlaylistDetailDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, id, EndpointPolicy.ReadInspect);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }
        
        var data = await playlistService.Load(id, cancellationToken);
        if (data is null)
        {
            return NotFound();
        }

        return Ok(TransferMaps.ToPlaylistDetailDto(data));
    }
}
