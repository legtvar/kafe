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
using Marten.Linq;

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
        return await db.LoadAsync<RoleInfo>(id.ToString(), token: token);
    }

    public async Task<ImmutableArray<RoleInfo>> LoadMany(
        IEnumerable<Hrib> ids,
        CancellationToken token = default)
    {
        return (await db.LoadManyAsync<RoleInfo>(token, ids.Select(i => i.ToString()))).ToImmutableArray();
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
        if (id == Hrib.Empty)
        {
            id = Hrib.Create();
        }

        var created = new RoleCreated(
            RoleId: id.ToString(),
            OrganizationId: @new.OrganizationId.ToString(),
            CreationMethod: @new.CreationMethod is not CreationMethod.Unknown
                ? @new.CreationMethod
                : CreationMethod.Api,
            Name: @new.Name
        );
        db.Events.KafeStartStream<RoleInfo>(id, created);

        if (!LocalizedString.IsNullOrEmpty(@new.Description))
        {
            var changed = new RoleInfoChanged(
                RoleId: id.ToString(),
                Name: null,
                Description: @new.Description
            );
            db.Events.KafeAppend(id, changed);
        }

        await db.SaveChangesAsync(token);

        return await db.Events.KafeAggregateRequiredStream<RoleInfo>(id, token: token);
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
            db.Events.Append(@old.Id, new RolePermissionSet(
                RoleId: @old.Id,
                EntityId: removedPermission,
                Permission: Permission.None
            ));
        }

        if (!hasChanged)
        {
            return Error.Unmodified($"role {modified.Id}");
        }

        await db.SaveChangesAsync(token);
        return await db.Events.KafeAggregateRequiredStream<RoleInfo>(@old.Id, token: token);
    }

    /// <summary>
    /// Gives the role the specified capabilities.
    /// </summary>
    public async Task<Err<bool>> AddPermissions(
        Hrib roleId,
        IEnumerable<(Hrib entityId, Permission permission)> permissions,
        CancellationToken token = default)
    {
        // TODO: Find a cheaper way of knowing that an account exists.
        var role = await Load(roleId, token);
        if (role is null)
        {
            return Error.NotFound(roleId, "A role");
        }

        foreach (var permissionPair in permissions)
        {
            if (!role.Permissions.TryGetValue(permissionPair.entityId.ToString(), out var existingPermission)
                || existingPermission != permissionPair.permission)
            {
                db.Events.KafeAppend(roleId, new RolePermissionSet(
                    RoleId: roleId.ToString(),
                    EntityId: permissionPair.entityId.ToString(),
                    Permission: permissionPair.permission
                ));
            }
        }
        await db.SaveChangesAsync(token);
        return true;
    }

    public Task<Err<bool>> AddPermissions(
        Hrib roleId,
        CancellationToken token = default,
        params (Hrib entityId, Permission permission)[] permissions)
    {
        return AddPermissions(roleId, permissions, token);
    }

    /// <summary>
    /// Filter of roles.
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
    public record RoleFilter(
        Hrib? AccessingAccountId = null,
        Hrib? OrganizationId = null,
        LocalizedString? Name = null
    );

    public async Task<ImmutableArray<RoleInfo>> List(
        RoleFilter? filter = null,
        CancellationToken token = default)
    {
        var query = db.Query<RoleInfo>();
        if (filter?.OrganizationId is not null)
        {
            query = (IMartenQueryable<RoleInfo>)query
                .Where(r => r.OrganizationId == filter.OrganizationId.ToString());
        }

        if (filter?.AccessingAccountId is not null)
        {
            query = (IMartenQueryable<RoleInfo>)query
                .WhereAccountHasPermission(
                    db.DocumentStore.Options.Schema,
                    Permission.Read,
                    filter.AccessingAccountId);
        }

        if (filter?.Name is not null)
        {
            query = (IMartenQueryable<RoleInfo>)query
                .WhereContainsLocalized(nameof(RoleInfo.Name), filter.Name);
        }

        return (await query.ToListAsync(token)).ToImmutableArray();
    }
}
