using Kafe.Api.Transfer;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public interface IArtifactService
{
    Task<ArtifactDetailDto?> Load(Hrib id, CancellationToken token = default);

    Task<ImmutableArray<ArtifactDetailDto?>> LoadMany(IEnumerable<Hrib> ids, CancellationToken token = default);

    Task<Hrib> Create(ArtifactCreationDto dto, CancellationToken token = default);
}
