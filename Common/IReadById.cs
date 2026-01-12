using System.Threading;
using System.Threading.Tasks;

namespace Kafe;

public interface IReadById<T>
    where T : IEntity
{
    Task<T?> Read(Hrib id, CancellationToken ct = default);
}
