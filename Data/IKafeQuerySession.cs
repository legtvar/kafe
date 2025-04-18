using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Aggregates;
using Marten;

namespace Kafe.Data;

public interface IKafeQuerySession : IQuerySession
{
    Task<Err<T>> LoadAsync<T>(
        Hrib id,
        CancellationToken token = default)
        where T : notnull, IEntity;

    Task<Err<ImmutableArray<T>>> LoadManyAsync<T>(
        ImmutableArray<Hrib> ids,
        CancellationToken token = default
    ) where T : notnull, IEntity;
}
