using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Aggregates;
using Kafe.Data.Documents;
using Marten;
using Marten.Linq.MatchesSql;

namespace Kafe.Data;

public static class KafeQueryable
{
    public static Task<IReadOnlyList<(T, EntityPermissionInfo)>> GetEntitiesWithPermission<T>(
        IDocumentSession db,
        Permission requiredPermission,
        Hrib accountId,
        CancellationToken token = default
    ) where T : IEntity
    {
        EnsureValidPermission(requiredPermission);
        EnsureValidAccountId(accountId);

        var schema = db.DocumentStore.Options.Schema;
        return db.AdvancedSql.QueryAsync<T, EntityPermissionInfo>(
            $"""
            SELECT
                ROW(entity.id, entity.data, entity.mt_version),
                ROW(perms.id, perms.data, perms.mt_version)
            FROM
                {schema.For<T>()} AS entity
            INNER JOIN
                {schema.For<EntityPermissionInfo>()} AS perms
            ON
                entity.id = perms.id
            WHERE
                (
                    (
                        (perms.data -> 'AccountEntries' -> ? -> 'EffectivePermission')::int
                        | perms.data -> 'GlobalPermission'
                    )
                    & ?
                ) = ?
            """,
            token,
            accountId.ToString(),
            (int)requiredPermission,
            (int)requiredPermission
        );
    }
    
    public static IQueryable<T> WhereAccountHasPermission<T>(
        this IQueryable<T> query,
        Permission requiredPermission,
        Hrib accountId
    ) where T : IEntity
    {
        EnsureValidPermission(requiredPermission);
        EnsureValidAccountId(accountId);

        return query.Where(e => e.MatchesSql(
            $"""
            (
                SELECT
                    (perms.data -> 'AccountEntries' -> ? -> 'EffectivePermission')::int
                        | (perms.data -> 'GlobalPermission')::int
                FROM mt_doc_entitypermissioninfo AS perms WHERE perms.id = d.id
            )::int & ? = ?
            """,
            accountId.ToString(),
            (int)requiredPermission,
            (int)requiredPermission));
    }
    
    private static void EnsureValidPermission(Permission requiredPermission)
    {
        if (requiredPermission == Permission.None)
        {
            throw new ArgumentException("The required permission must not be None.", nameof(requiredPermission));
        }
    }

    private static void EnsureValidAccountId(Hrib accountId)
    {
        if (!accountId.IsValidNonEmpty)
        {
            throw new ArgumentException("The account id must be a non-empty valid HRIB.", nameof(accountId));
        }
    }
}
