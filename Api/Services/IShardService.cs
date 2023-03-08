using Kafe.Api.Transfer;
using Kafe.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public interface IShardService
{
    Task<ShardDetailBaseDto?> Load(Hrib id, CancellationToken token = default);

    Task<Hrib?> Create(
        ShardCreationDto dto,
        Stream stream,
        CancellationToken token = default);

    Task<ShardKind> GetShardKind(Hrib id, CancellationToken token = default);

    Task<ShardVariantMediaTypeDto?> GetShardVariantMediaType(
        Hrib id,
        string? variant,
        CancellationToken token = default);

    Task<Stream> OpenStream(Hrib id, string? variant, CancellationToken token = default);
}
