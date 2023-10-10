using Kafe.Api.Transfer;
using Kafe.Data.Aggregates;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public interface IAuthorService
{
    Task<AuthorDetailDto?> Load(Hrib id, CancellationToken token = default);

    Task<ImmutableArray<AuthorListDto>> List(CancellationToken token = default);

    Task<AuthorInfo> Create(AuthorCreationDto dto, Hrib? ownerId, CancellationToken token = default);
}
