using Kafe.Api.Transfer;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public interface IProjectService
{
    Task<ProjectDetailDto?> Load(Hrib id, CancellationToken token = default);

    Task<ImmutableArray<ProjectListDto>> List(CancellationToken token = default);

    Task<Hrib> Create(ProjectCreationDto dto, CancellationToken token = default);

    Task<ProjectValidationDto> Validate(Hrib id, CancellationToken token = default);

    Task Review(ProjectReviewCreationDto dto, CancellationToken token = default);
}
