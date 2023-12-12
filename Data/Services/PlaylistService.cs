using Kafe.Data.Aggregates;
using Marten;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class PlaylistService
{
    private readonly IDocumentSession db;
    public PlaylistService(IDocumentSession db)
    {
        this.db = db;
    }

    public async Task<ImmutableArray<PlaylistInfo>> List(CancellationToken token = default)
    {
        // TODO: Return list of artifacts referenced in the playlist.

        return (await db.Query<PlaylistInfo>().ToListAsync(token)).ToImmutableArray();
    }

    public async Task<PlaylistInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<PlaylistInfo>(id.Value, token);
    }
}
