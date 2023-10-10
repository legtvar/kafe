using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Aggregates;
using Marten;

namespace Kafe.Data.Services;

public class EntityService
{
    private readonly IDocumentSession db;

    public EntityService(IDocumentSession db)
    {
        this.db = db;
    }

    public async Task<IEntity?> Load(Hrib id, CancellationToken token = default)
    {
        var parentState = await db.Events.FetchStreamStateAsync(id);
        if (parentState?.AggregateType is null)
        {
            return null;
        }

        return (await db.QueryAsync(
            parentState.AggregateType,
            "where data ->> Id = ?",
            token,
            parentState.Id))
                .Cast<IEntity>()
                .FirstOrDefault();
    }
}
