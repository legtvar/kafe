using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Common;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Data.Metadata;
using Marten;
using Marten.Linq;

namespace Kafe.Data.Services;

public class OrganizationService
{
    private readonly IDocumentSession db;
    private readonly EntityMetadataProvider entityMetadataProvider;

    public OrganizationService(
        IDocumentSession db,
        EntityMetadataProvider entityMetadataProvider)
    {
        this.db = db;
        this.entityMetadataProvider = entityMetadataProvider;
    }

    public async Task<OrganizationInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<OrganizationInfo>(id.ToString(), token: token);
    }

    public async Task<ImmutableArray<OrganizationInfo>> LoadMany(
        IEnumerable<Hrib> ids,
        CancellationToken token = default)
    {
        return (await db.KafeLoadManyAsync<OrganizationInfo>(ids.ToImmutableArray(), token)).Unwrap();
    }

    public async Task<Err<OrganizationInfo>> Create(OrganizationInfo @new, CancellationToken token = default)
    {
        var parseResult = Hrib.Parse(@new.Id);
        if (parseResult.HasErrors)
        {
            return parseResult.Errors;
        }

        var id = parseResult.Value;
        if (id == Hrib.Empty)
        {
            id = Hrib.Create();
        }

        var created = new OrganizationCreated(
            OrganizationId: id.ToString(),
            CreationMethod: @new.CreationMethod is not CreationMethod.Unknown
                ? @new.CreationMethod
                : CreationMethod.Api,
            Name: @new.Name
        );
        db.Events.KafeStartStream<OrganizationInfo>(id, created);
        await db.SaveChangesAsync(token);

        return await db.Events.KafeAggregateRequiredStream<OrganizationInfo>(id, token: token);
    }

    public async Task<Err<OrganizationInfo>> Edit(OrganizationInfo modified, CancellationToken token = default)
    {
        var @old = await Load(modified.Id, token);
        if (@old is null)
        {
            return Error.NotFound(modified.Id);
        }

        if ((LocalizedString)@old.Name != modified.Name)
        {
            db.Events.Append(@old.Id, new OrganizationInfoChanged(
                OrganizationId: @old.Id,
                Name: modified.Name
            ));
            await db.SaveChangesAsync(token);
            return await db.Events.AggregateStreamAsync<OrganizationInfo>(@old.Id, token: token)
                ?? throw new InvalidOperationException($"The organization is no longer present in the database. "
                    + "This should never happen.");
        }

        return Error.Unmodified($"organization {modified.Id}");
    }
    
    /// <summary>
    /// Filter of organizations.
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
    public record OrganizationFilter(
        Hrib? AccessingAccountId = null,
        LocalizedString? Name = null
    );

    public async Task<ImmutableArray<OrganizationInfo>> List(
        OrganizationFilter? filter = null,
        string? sort = null,
        CancellationToken token = default)
    {
        var query = db.Query<OrganizationInfo>();
        if (filter?.AccessingAccountId is not null)
        {
            query = (IMartenQueryable<OrganizationInfo>)query
                .WhereAccountHasPermission(
                    db.DocumentStore.Options.Schema,
                    Permission.Read,
                    filter.AccessingAccountId);
        }

        if (filter?.Name is not null)
        {
            query = (IMartenQueryable<OrganizationInfo>)query
                .WhereContainsLocalized(nameof(OrganizationInfo.Name), filter.Name);
        }
        
        if (!string.IsNullOrEmpty(sort))
        {
            query = (IMartenQueryable<OrganizationInfo>)query.OrderBySortString(entityMetadataProvider, sort);
        }

        return (await query.ToListAsync(token)).ToImmutableArray();
    }
}
