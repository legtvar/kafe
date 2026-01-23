using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Data.Metadata;
using Marten;
using Marten.Linq;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class ProjectGroupService(
    IDocumentSession db,
    EntityMetadataProvider entityMetadataProvider
)
{
    public async Task<Err<ProjectGroupInfo>> Create(
        ProjectGroupInfo @new,
        bool shouldWaitForDaemon = true,
        CancellationToken token = default
    )
    {
        if (!Hrib.TryParse(@new.Id, out var id, out _))
        {
            return Err.Fail<ProjectGroupInfo>(new BadHribDiagnostic(@new.Id));
        }

        if (!Hrib.TryParse(@new.OrganizationId, out var organizationId, out _))
        {
            return Err.Fail<ProjectGroupInfo>(new BadHribDiagnostic(@new.OrganizationId));
        }

        var orgErr = await db.KafeLoadAsync<OrganizationInfo>(organizationId, token);
        if (orgErr.HasError)
        {
            return orgErr.Diagnostic;
        }

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

        if (@new.ValidationSettings is not null)
        {
            var validationSettingsChanged = new ProjectGroupValidationSettingsChanged(
                ProjectGroupId: id.ToString(),
                ValidationSettings: @new.ValidationSettings
            );
            db.Events.KafeAppend(id, validationSettingsChanged);
        }

        await db.SaveChangesAsync(token);

        if (shouldWaitForDaemon)
        {
            await db.TryWaitForEntityPermissions(id, token);
        }

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

    public async Task<Err<ProjectGroupInfo>> Load(Hrib id, CancellationToken token = default)
    {
        return await db.KafeLoadAsync<ProjectGroupInfo>(id, token);
    }

    public async Task<Err<ProjectGroupInfo>> Edit(ProjectGroupInfo @new, CancellationToken token = default)
    {
        if (!Hrib.TryParse(@new.Id, out var id, out _))
        {
            return Err.Fail<ProjectGroupInfo>(new BadHribDiagnostic(@new.Id));
        }

        var oldErr = await db.KafeLoadAsync<ProjectGroupInfo>(id, token);
        if (oldErr.HasError)
        {
            return oldErr;
        }

        var @old = oldErr.Value;

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
            var organizationIdErr = Hrib.TryParseValid(@new.OrganizationId);
            if (organizationIdErr.HasError)
            {
                return organizationIdErr.Diagnostic.ForParameter(nameof(@new.OrganizationId));
            }

            eventStream.AppendOne(new ProjectGroupMovedToOrganization(
                @old.Id,
                @new.OrganizationId
            ));
        }

        if (@old.ValidationSettings != @new.ValidationSettings)
        {
            eventStream.AppendOne(new ProjectGroupValidationSettingsChanged(
                @old.Id,
                @new.ValidationSettings
            ));
        }

        await db.SaveChangesAsync(token);
        return await db.Events.AggregateStreamAsync<ProjectGroupInfo>(@old.Id, token: token)
               ?? throw new InvalidOperationException($"The project group is no longer present in the database. "
                                                      + "This should never happen.");
    }
}
