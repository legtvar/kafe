using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe;

public interface IFindShardFile
{
    Task<Err<string>> Find(Uri shardUri, CancellationToken ct = default);
}
