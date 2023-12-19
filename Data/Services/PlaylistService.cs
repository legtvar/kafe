using Kafe.Data.Aggregates;
using Marten;
using Marten.Linq;
using Marten.Linq.MatchesSql;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

    public record PlaylistFilter(
        Hrib? AccessingAccountId
    );

    public async Task<ImmutableArray<PlaylistInfo>> List(
        PlaylistFilter? filter = null,
        CancellationToken token = default)
    {
        var query = db.Query<PlaylistInfo>();

        if (filter?.AccessingAccountId is not null)
        {
            query = (IMartenQueryable<PlaylistInfo>)query
                .Where(e => e.MatchesSql(
                    $"({SqlFunctions.GetPlaylistPerms}(data ->> 'Id', ?) & ?) != 0",
                    filter.AccessingAccountId.Value,
                    (int)Permission.Read));
        }

        return (await query.ToListAsync(token)).ToImmutableArray();
    }

    public async Task<PlaylistInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<PlaylistInfo>(id.Value, token);
    }

    public async Task<ImmutableArray<PlaylistInfo>> LoadMany(
        IEnumerable<Hrib> ids,
        CancellationToken token = default)
    {
        return (await db.LoadManyAsync<PlaylistInfo>(token, ids.Select(i => (string)i)))
            .Where(a => a is not null)
            .ToImmutableArray();
    }
}
