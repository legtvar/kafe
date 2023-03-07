using Kafe.Api.Transfer;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public interface IPlaylistService
{
    Task<PlaylistDetailDto?> Load(Hrib id, CancellationToken token = default);

    Task<ImmutableArray<PlaylistListDto>> List(CancellationToken token = default);
}
