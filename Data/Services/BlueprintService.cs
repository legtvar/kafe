using Kafe.Data.Aggregates;
using Marten;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class BlueprintService(
    IDocumentSession db
)
{
    public async Task<Err<BlueprintInfo>> Load(Hrib id, CancellationToken token = default)
    {
        return await db.KafeLoadAsync<BlueprintInfo>(id, token);
    }

    public async Task<Err<ImmutableArray<BlueprintInfo>>> LoadMany(
        IReadOnlyList<Hrib> ids,
        CancellationToken token = default
    )
    {
        return await db.KafeLoadManyAsync<BlueprintInfo>(ids, token);
    }
}
