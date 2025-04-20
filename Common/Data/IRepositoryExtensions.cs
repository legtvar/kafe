using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe;

public static class IRepositoryExtensions
{
    public static async ValueTask<Err<T>> Read<T>(
        this IRepository<T> repo,
        Hrib id,
        CancellationToken ct = default
    )
        where T : IEntity
    {
        var err = await repo.Read([id], ct);
        if (err.HasValue)
        {
            return new Err<T>(err.Value.SingleOrDefault(), err.Diagnostic);
        }

        return new Err<T>(default, err.Diagnostic);
    }
}
