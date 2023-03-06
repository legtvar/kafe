using Kafe.Api.Transfer;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public interface IProjectGroupService
{
    Task<ProjectGroupDetailDto?> Load(Hrib id, CancellationToken token = default);

    Task<ImmutableArray<ProjectGroupListDto>> List(CancellationToken token = default);

    Task<Hrib> Create(ProjectGroupCreationDto dto, CancellationToken token = default);
}
