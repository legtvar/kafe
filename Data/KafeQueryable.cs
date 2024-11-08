using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Aggregates;
using Kafe.Data.Documents;
using Marten;
using Marten.Linq;
using Marten.Linq.MatchesSql;

namespace Kafe.Data;

public static class KafeQueryable
{
    public static Task<IReadOnlyList<(T, EntityPermissionInfo)>> GetEntitiesWithPermission<T>(
        IQuerySession db,
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
        IDocumentSchemaResolver schema,
        Permission requiredPermission,
        Hrib accountId
    ) where T : IEntity
    {
        EnsureValidPermission(requiredPermission);
        EnsureValidAccountId(accountId);

        // NB: Anonymous users
        if (accountId.IsEmpty)
        {
            var anonSql =
            $"""
            (
                SELECT (perms.data -> 'GlobalPermission')::int
                FROM {schema.For<EntityPermissionInfo>()} AS perms
                WHERE perms.id = d.id
            )::int & ? = ?
            """;

            return query.Where(e => e.MatchesSql(
                anonSql,
                (int)requiredPermission,
                (int)requiredPermission));
        }

        var sql = $"""
        (
            SELECT
                (perms.data -> 'AccountEntries' -> ? -> 'EffectivePermission')::int
                    | (perms.data -> 'GlobalPermission')::int
            FROM {schema.For<EntityPermissionInfo>()} AS perms
            WHERE perms.id = d.id
        )::int & ? = ?
        """;
        return query.Where(e => e.MatchesSql(
            sql,
            accountId.ToString(),
            (int)requiredPermission,
            (int)requiredPermission));
    }

    public static IQueryable<T> WhereContainsLocalized<T>(
        this IQueryable<T> query,
        string fieldName,
        LocalizedString value
    )
        where T : IEntity
    {
        var dictName = (ImmutableDictionary<string, string>)value;
        query = query.Where(e => e.MatchesSql(
                $"data -> {fieldName} @> (?)::jsonb",
                dictName));
        return query;
    }

    public static IQueryable<T> OrderBySortString<T>(
        this IQueryable<T> query,
        string sortString,
        bool isDescending = false
    ) where T : IEntity
    {
        var fragments = sortString.Trim().Split('.')
            .Select(f => f.Trim())
            .Where(f => !string.IsNullOrEmpty(f))
            .Select(f =>
            {
                var sb = new StringBuilder();
                sb.Append('\'');
                sb.Append(char.ToUpper(f[0]));
                sb.Append(f[1..]);
                sb.Append('\'');
                return sb.ToString();
            })
            .ToArray();
        var sql = new StringBuilder();
        sql.Append("d.data");
        if (fragments.Length > 1)
        {
            sql.Append(" -> ");
            sql.Append(string.Join(" -> ", fragments[..^1]));
        }

        sql.Append(" ->> ");
        sql.Append(fragments[^1]);

        if (isDescending)
        {
            sql.Append(" DESC");
        }
        
        return query.OrderBySql(sql.ToString());
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
        if (accountId.IsInvalid)
        {
            throw new ArgumentException("The account id must be a valid HRIB.", nameof(accountId));
        }
    }
}
