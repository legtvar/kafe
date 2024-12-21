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
using System.Linq;
using JasperFx.Core;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Http;

namespace Kafe.Api.Endpoints.Playlist;

[ApiVersion("1")]
[Route("playlist/{id}")]
public class PlaylistDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<PlaylistDetailDto>
{
    private readonly PlaylistService playlistService;
    private readonly ArtifactService artifactService;
    private readonly IAuthorizationService authorizationService;

    public PlaylistDetailEndpoint(
        PlaylistService playlistService,
        ArtifactService artifactService,
        IAuthorizationService authorizationService)
    {
        this.playlistService = playlistService;
        this.artifactService = artifactService;
        this.authorizationService = authorizationService;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Playlist })]
    [Tags(EndpointArea.Playlist)]
    [ProducesResponseType(typeof(PlaylistDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<PlaylistDetailDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, id, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var data = await playlistService.Load(id, cancellationToken);
        if (data is null)
        {
            return NotFound();
        }

        var dto = TransferMaps.ToPlaylistDetailDto(data);

        var artifacts = await artifactService
            .LoadMany(data.EntryIds.Select(i => (Hrib)i), cancellationToken);
        dto = dto with
        {
            Entries = artifacts.Select(a => new PlaylistEntryDto((Hrib)a.Id, (LocalizedString)a.Name))
                .ToImmutableArray()
        };

        return Ok(dto);
    }
}
