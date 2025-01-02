#pragma warning disable 0618

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Aggregates;
using Kafe.Data.Documents;
using Marten;
using Marten.Linq.MatchesSql;

namespace Kafe.Data.Events.Corrections;

/// <summary>
/// If there are any events prior to the date of the correction, adds a `legacy--org` organization that becomes the
/// owner of all pre-existing project groups and playlists.
/// </summary>
[AutoCorrection("2025-01-01")]
internal class ProjectCreatorInspectPermissionCorrection : IEventCorrection
{
    public async Task Apply(IDocumentSession db, CancellationToken ct = default)
    {
        var projectFilter =
        $"""
        (
            SELECT stream.type
            FROM {db.DocumentStore.Options.Schema.ForStreams()} as stream
            WHERE stream.id = d.id
        ) = 'project_info'
        """;

        var explicitAccountPermsFilter =
        $"""
        json_exists(d.data, '$.AccountEntries.*.Sources."' || d.id || '"')
        """;

        var affectedProjects = db.Query<EntityPermissionInfo>()
            .Where(p => p.MatchesSql(projectFilter) && p.MatchesSql(explicitAccountPermsFilter))
            .ToAsyncEnumerable(ct);
        await foreach (var affected in affectedProjects)
        {
            foreach (var (accountId, accountEntry) in affected.AccountEntries
                .Where(p => p.Value.Sources.ContainsKey(affected.Id)))
            {
                if (accountEntry.Sources[affected.Id].Permission.HasFlag(Permission.Read)
                    && !accountEntry.Sources[affected.Id].Permission.HasFlag(Permission.Inspect))
                {
                    db.Events.KafeAppend(accountId, new AccountPermissionSet(
                        accountId,
                        affected.Id,
                        Permission.Inspect
                    ));
                }
            }
        }
    }
}
