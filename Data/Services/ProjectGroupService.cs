using Kafe.Common;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using Marten.Linq;
using Marten.Linq.MatchesSql;
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
            ProjectGroupId: id.Value,
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

    public record ProjectGroupFilter(
        Hrib? AccessingAccountId = null
    );

    public async Task<ImmutableArray<ProjectGroupInfo>> List(
        ProjectGroupFilter? filter = null,
        CancellationToken token = default)
    {
        var query = db.Query<ProjectGroupInfo>();
        if (filter?.AccessingAccountId is not null)
        {
            query = (IMartenQueryable<ProjectGroupInfo>)query
                .Where(e => e.MatchesSql(
                    $"({SqlFunctions.GetProjectGroupPerms}(data ->> 'Id', ?) & ?) != 0",
                    filter.AccessingAccountId.Value,
                    (int)Permission.Read));
        }

        return (await query.ToListAsync(token)).ToImmutableArray();
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
        return await db.LoadAsync<ProjectGroupInfo>(id.Value, token);
    }

    public async Task<Err<bool>> Edit(ProjectGroupInfo @new, CancellationToken token = default)
    {
        var @old = await Load(@new.Id, token);
        if (@old is null)
        {
            return Err.NotFound<bool>(@new.Id);
        }

        var eventStream = await db.Events.FetchForExclusiveWriting<ProjectGroupInfo>(@new.Id, token);

        var infoChanged = new ProjectGroupInfoChanged(
            ProjectGroupId: @new.Id,
            Name: (LocalizedString)@old.Name != @new.Name ? @new.Name : null,
            Description: (LocalizedString?)@old.Description != @new.Description ? @new.Description : null,
            Deadline: @old.Deadline != @new.Deadline ? @new.Deadline : null);
        if (infoChanged.Name is not null
            || infoChanged.Description is not null
            || infoChanged.Deadline is not null)
        {
            eventStream.AppendOne(infoChanged);
        }
        
        if (@old.IsOpen != @new.IsOpen)
        {
            eventStream.AppendOne(@new.IsOpen
                ? new ProjectGroupOpened(@new.Id)
                : new ProjectGroupClosed(@new.Id));
        }
        
        await db.SaveChangesAsync(token);
        return true;
    }
}
