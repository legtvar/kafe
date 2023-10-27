using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class ProjectGroupService
{
    private readonly IDocumentSession db;

    public ProjectGroupService(IDocumentSession db)
    {
        this.db = db;
    }

    public async Task<Hrib> Create(
        LocalizedString name,
        LocalizedString? description,
        DateTimeOffset deadline,
        Hrib? id = null,
        CancellationToken token = default)
    {
        id ??= Hrib.Create();
        var created = new ProjectGroupCreated(
            ProjectGroupId: id,
            CreationMethod: CreationMethod.Api,
            Name: name);

        var changed = new ProjectGroupInfoChanged(
            ProjectGroupId: created.ProjectGroupId,
            Description: description,
            Deadline: deadline);

        var opened = new ProjectGroupOpened(
            ProjectGroupId: created.ProjectGroupId);

        db.Events.StartStream<ProjectGroupInfo>(created.ProjectGroupId, created, changed, opened);
        await db.SaveChangesAsync(token);
        return created.ProjectGroupId;
    }

    public async Task<ImmutableArray<ProjectGroupInfo>> List(CancellationToken token = default)
    {
        return (await db.Query<ProjectGroupInfo>().ToListAsync(token)).ToImmutableArray();
    }

    // TODO: Search
    // public async Task<ImmutableArray<ProjectGroupListDto>> List(
    //     LocalizedString name,
    //     CancellationToken token = default)
    // {
    //     var projectGroups = await db.Query<ProjectGroupInfo>()
    //         .WhereCanRead(userProvider)
    //         .Where(g => g.Name[Const.InvariantCultureCode] == name[Const.InvariantCultureCode])
    //         .ToListAsync(token);

    //     return projectGroups
    //         .Select(TransferMaps.ToProjectGroupListDto).ToImmutableArray();
    // }

    public async Task<ProjectGroupInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<ProjectGroupInfo>(id, token);

        // if (!userProvider.CanRead(projectGroup))
        // {
        //     throw new UnauthorizedAccessException();
        // }
    }
}
