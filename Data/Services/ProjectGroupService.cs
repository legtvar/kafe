using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Data.Metadata;
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
    private readonly OrganizationService organizationService;
    private readonly EntityMetadataProvider entityMetadataProvider;

    public ProjectGroupService(
        IDocumentSession db,
        OrganizationService organizationService,
        EntityMetadataProvider entityMetadataProvider)
    {
        this.db = db;
        this.organizationService = organizationService;
        this.entityMetadataProvider = entityMetadataProvider;
    }

    public async Task<Err<ProjectGroupInfo>> Create(
        ProjectGroupInfo @new,
        CancellationToken token = default)
    {
        var parseResult = Hrib.Parse(@new.Id);
        if (parseResult.HasErrors)
        {
            return parseResult.Errors;
        }

        var organization = await organizationService.Load(@new.OrganizationId, token);
        if (organization is null)
        {
            return Kafe.Diagnostic.NotFound(@new.OrganizationId, "An organization");
        }

        var id = parseResult.Value;
        if (id == Hrib.Empty)
        {
            id = Hrib.Create();
        }

        var created = new ProjectGroupCreated(
            ProjectGroupId: id.ToString(),
            CreationMethod: @new.CreationMethod is not CreationMethod.Unknown
                ? @new.CreationMethod
                : CreationMethod.Api,
            OrganizationId: @new.OrganizationId,
            Name: @new.Name);
        db.Events.KafeStartStream<ProjectGroupInfo>(id, created);

        if (@new.Description is not null
            || @new.Deadline != default)
        {
            var changed = new ProjectGroupInfoChanged(
                ProjectGroupId: created.ProjectGroupId,
                Description: @new.Description,
                Deadline: @new.Deadline);
            db.Events.KafeAppend(id, changed);
        }

        if (@new.IsOpen)
        {
            var opened = new ProjectGroupOpened(
                ProjectGroupId: created.ProjectGroupId);
            db.Events.KafeAppend(id, opened);
        }

        await db.SaveChangesAsync(token);
        return await db.Events.KafeAggregateRequiredStream<ProjectGroupInfo>(id, token: token);
    }

    /// <summary>
    /// Filter of project groups.
    /// </summary>
    /// <param name="AccessingAccountId">
    /// <list type="bullet">
    /// <item> If null, doesn't filter by account access at all.</item>
    /// <item>
    ///     If <see cref="Hrib.Empty"/> assumes the account is an anonymous user
    ///     and filters only by global permissions.
    /// </item>
    /// <item> If <see cref="Hrib.Invalid"/>, throws an exception. </item>
    /// </list>
    /// </param>
    public record ProjectGroupFilter(
        Hrib? AccessingAccountId = null,
        Hrib? OrganizationId = null,
        LocalizedString? Name = null
    );

    public async Task<ImmutableArray<ProjectGroupInfo>> List(
        ProjectGroupFilter? filter = null,
        string? sort = null,
        CancellationToken token = default)
    {
        var query = db.Query<ProjectGroupInfo>();
        if (filter?.AccessingAccountId is not null)
        {
            query = (IMartenQueryable<ProjectGroupInfo>)query
                .WhereAccountHasPermission(
                    db.DocumentStore.Options.Schema,
                    Permission.Read,
                    filter.AccessingAccountId);
        }

        if (filter?.OrganizationId is not null)
        {
            query = (IMartenQueryable<ProjectGroupInfo>)query
                .Where(g => g.OrganizationId == filter.OrganizationId.ToString());
        }

        if (filter?.Name is not null)
        {
            query = (IMartenQueryable<ProjectGroupInfo>)query
                .WhereContainsLocalized(nameof(ProjectGroupInfo.Name), filter.Name);
        }

        if (!string.IsNullOrEmpty(sort))
        {
            query = (IMartenQueryable<ProjectGroupInfo>)query.OrderBySortString(entityMetadataProvider, sort);
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
        return await db.LoadAsync<ProjectGroupInfo>(id.ToString(), token);
    }

    public async Task<Err<ProjectGroupInfo>> Edit(ProjectGroupInfo @new, CancellationToken token = default)
    {
        var @old = await Load(@new.Id, token);
        if (@old is null)
        {
            return Kafe.Diagnostic.NotFound(@new.Id);
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

        if (@old.OrganizationId != @new.OrganizationId)
        {
            if (!((Hrib)@new.OrganizationId).IsValidNonEmpty)
            {
                return Kafe.Diagnostic.InvalidOrEmptyHrib(nameof(ProjectGroupInfo.OrganizationId));
            }

            eventStream.AppendOne(new ProjectGroupMovedToOrganization(
                @old.Id,
                @new.OrganizationId
            ));
        }

        await db.SaveChangesAsync(token);
        return await db.Events.AggregateStreamAsync<ProjectGroupInfo>(@old.Id, token: token)
            ?? throw new InvalidOperationException($"The project group is no longer present in the database. "
                + "This should never happen.");
    }

    public async Task<Err<ProjectGroupInfo>> CreateOrEdit(ProjectGroupInfo info, CancellationToken token = default)
    {
        // TODO: Get rid of the unnecessary trip to DB (by calling Load twice).
        var existing = info.Id == Hrib.InvalidValue ? null : await Load(info.Id, token);
        return existing is null ? await Create(info, token) : await Edit(info, token);
    }
}
