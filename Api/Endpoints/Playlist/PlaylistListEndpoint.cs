using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Api.Services;
using Kafe.Data.Services;
using System.Linq;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Http;

namespace Kafe.Api.Endpoints.Playlist;

[ApiVersion("1")]
[Route("playlists")]
public class PlaylistListEndpoint : EndpointBaseAsync
    .WithRequest<PlaylistListEndpoint.RequestData>
    .WithActionResult<List<PlaylistListDto>>
{
    private readonly PlaylistService playlistService;
    private readonly IAuthorizationService authorization;
    private readonly UserProvider userProvider;

    public PlaylistListEndpoint(
        PlaylistService playlistService,
        IAuthorizationService authorization,
        UserProvider userProvider)
    {
        this.playlistService = playlistService;
        this.authorization = authorization;
        this.userProvider = userProvider;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Playlist })]
    [Tags(EndpointArea.Playlist)]
    public override async Task<ActionResult<List<PlaylistListDto>>> HandleAsync(
        RequestData requestData,
        CancellationToken cancellationToken = default)
    {
        var filter = new PlaylistService.PlaylistFilter(
            AccessingAccountId: userProvider.AccountId,
            OrganizationId: requestData.OrganizationId
        );
        var list = await playlistService.List(filter, requestData.Sort, cancellationToken);
        return Ok(list.Select(TransferMaps.ToPlaylistListDto).ToImmutableArray());
    }

    public record RequestData
    {
        [FromQuery(Name = "organization")]
        public string? OrganizationId { get; set; }

        [FromQuery(Name = "sort")]
        public string? Sort { get; set; } = "name.iv";
    }
}
