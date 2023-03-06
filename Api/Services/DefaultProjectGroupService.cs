using Kafe.Api.Transfer;
using Kafe.Data.Aggregates;
using Marten;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DefaultProjectGroupService : IProjectGroupService
{
    private readonly IDocumentSession db;
    private readonly IUserProvider userProvider;

    public DefaultProjectGroupService(
        IDocumentSession db,
        IUserProvider userProvider)
    {
        this.db = db;
        this.userProvider = userProvider;
    }

    public Task<Hrib> Create(ProjectGroupCreationDto dto, CancellationToken token = default)
    {
        throw new System.NotImplementedException();
    }

    public async Task<ImmutableArray<ProjectGroupListDto>> List(CancellationToken token = default)
    {
        var preferredCulture = userProvider.User!.PreferredCulture;
        var projectGroups = await db.Query<ProjectGroupInfo>()
            .ToListAsync(token);

        return projectGroups
            .Select(TransferMaps.ToProjectGroupListDto).ToImmutableArray();
    }

    public Task<ProjectGroupDetailDto?> Load(Hrib id, CancellationToken token = default)
    {
        throw new System.NotImplementedException();
    }
}
