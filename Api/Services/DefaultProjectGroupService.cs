using Kafe.Api.Transfer;
using Kafe.Data.Aggregates;
using Marten;
using System;
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
            .WhereCanRead(userProvider)
            .ToListAsync(token);

        return projectGroups
            .Select(TransferMaps.ToProjectGroupListDto).ToImmutableArray();
    }

    public async Task<ProjectGroupDetailDto?> Load(Hrib id, CancellationToken token = default)
    {
        var projectGroup = await db.LoadAsync<ProjectGroupInfo>(id, token);
        if (projectGroup is null)
        {
            return null;
        }

        if (userProvider.CanRead(projectGroup))
        {
            throw new UnauthorizedAccessException();
        }

        var dto = TransferMaps.ToProjectGroupDetailDto(projectGroup);
        var projects = await db.Query<ProjectInfo>()
            .Where(p => p.ProjectGroupId == projectGroup.Id)
            .WhereCanRead(userProvider)
            .ToListAsync(token);
        var preferredCulture = userProvider.GetPreferredCulture();
        return dto with
        {
            Projects = projects
                .OrderBy(p => p.Name[preferredCulture])
                .Select(TransferMaps.ToProjectListDto)
                .ToImmutableArray()
        };
    }
}
