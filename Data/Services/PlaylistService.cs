using Kafe.Api.Transfer;
using Kafe.Data.Aggregates;
using Marten;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class PlaylistService
{
    private readonly IDocumentSession db;
    private readonly IUserProvider userProvider;

    public PlaylistService(
        IDocumentSession db,
        IUserProvider userProvider)
    {
        this.db = db;
        this.userProvider = userProvider;
    }

    public async Task<ImmutableArray<PlaylistListDto>> List(CancellationToken token = default)
    {
        // TODO: Return list of artifacts referenced in the playlist.

        var playlists = await db.Query<PlaylistInfo>()
            .WhereCanRead(userProvider)
            .ToListAsync(token);

        return playlists.Select(TransferMaps.ToPlaylistListDto).ToImmutableArray();
    }

    public async Task<PlaylistDetailDto?> Load(Hrib id, CancellationToken token = default)
    {
        var data = await db.LoadAsync<PlaylistInfo>(id, token);
        if (data is null)
        {
            return null;
        }

        if (!userProvider.CanRead(data))
        {
            throw new UnauthorizedAccessException();
        }

        return TransferMaps.ToPlaylistDetailDto(data);
    }
}
