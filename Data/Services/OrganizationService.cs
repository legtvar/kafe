using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Common;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;

namespace Kafe.Data.Services;

public class OrganizationService
{
    private readonly IDocumentSession db;

    public OrganizationService(IDocumentSession db)
    {
        this.db = db;
    }

    public async Task<OrganizationInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<OrganizationInfo>(id.ToString(), token: token);
    }

    public async Task<ImmutableArray<OrganizationInfo>> LoadMany(
        IEnumerable<Hrib> ids,
        CancellationToken token = default)
    {
        return (await db.LoadManyAsync<OrganizationInfo>(token, ids.Select(i => i.ToString()))).ToImmutableArray();
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
            CreationMethod: CreationMethod.Api,
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
}
