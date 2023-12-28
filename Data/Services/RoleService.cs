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

public class RoleService
{
    private readonly IDocumentSession db;
    private readonly OrganizationService organizationService;

    public RoleService(
        IDocumentSession db,
        OrganizationService organizationService)
    {
        this.db = db;
        this.organizationService = organizationService;
    }

    public async Task<RoleInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<RoleInfo>(id.Value, token: token);
    }

    public async Task<ImmutableArray<RoleInfo>> LoadMany(
        IEnumerable<Hrib> ids,
        CancellationToken token = default)
    {
        return (await db.LoadManyAsync<RoleInfo>(token, ids.Select(i => i.Value))).ToImmutableArray();
    }

    public async Task<Err<RoleInfo>> Create(RoleInfo @new, CancellationToken token = default)
    {
        var parseResult = Hrib.Parse(@new.Id);
        if (parseResult.HasErrors)
        {
            return parseResult.Errors;
        }

        var organization = await organizationService.Load(@new.OrganizationId, token);
        if (organization is null)
        {
            return Error.NotFound(@new.OrganizationId, "An organization");
        }

        var id = parseResult.Value;
        if (id == Hrib.Invalid)
        {
            id = Hrib.Create();
        }

        var created = new RoleCreated(
            RoleId: id.Value,
            OrganizationId: id.Value,
            CreationMethod: CreationMethod.Api,
            Name: @new.Name
        );
        db.Events.StartStream<RoleInfo>(id.Value, created);
        await db.SaveChangesAsync(token);

        return await db.Events.AggregateStreamAsync<RoleInfo>(id.Value, token: token)
            ?? throw new InvalidOperationException($"Could not persist a role with id '{id.Value}'.");
    }

    public async Task<Err<RoleInfo>> Edit(RoleInfo modified, CancellationToken token = default)
    {
        var @old = await Load(modified.Id, token);
        if (@old is null)
        {
            return Error.NotFound(modified.Id);
        }

        var hasChanged = false;
        if ((LocalizedString)@old.Name != modified.Name
            || (LocalizedString?)@old.Description != modified.Description)
        {
            hasChanged = true;
            db.Events.Append(@old.Id, new RoleInfoChanged(
                RoleId: @old.Id,
                Name: modified.Name,
                Description: modified.Description
            ));
        }

        var changedPermissions = modified.Permissions.Except(@old.Permissions);
        foreach (var changedPermission in changedPermissions)
        {
            hasChanged = true;
            db.Events.Append(@old.Id, new RolePermissionSet(
                RoleId: @old.Id,
                EntityId: changedPermission.Key,
                Permission: changedPermission.Value
            ));
        }

        var removedPermissions = @old.Permissions.Keys.Except(modified.Permissions.Keys);
        foreach (var removedPermission in removedPermissions)
        {
            hasChanged = true;
            db.Events.Append(@old.Id, new RolePermissionUnset(
                RoleId: @old.Id,
                EntityId: removedPermission
            ));
        }

        if (!hasChanged)
        {
            return Error.Unmodified($"role {modified.Id}");
        }

        await db.SaveChangesAsync(token);
        return await db.Events.AggregateStreamAsync<RoleInfo>(@old.Id, token: token)
            ?? throw new InvalidOperationException($"The role is no longer present in the database. "
                + "This should never happen.");
    }
}
