using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Marten.Schema;

namespace Kafe.Data;

public class NaturalSortCollation : IInitialData
{
    public const string QualifiedName = "public.kafe_natural";

    public async Task Populate(IDocumentStore store, CancellationToken token)
    {
        await using var session = store.LightweightSession();
        // NB: 'und-u-kn-true' is a BCP 47 language tag:
        //     "und" = "undetermined" locale (equivalent of InvariantCulture)
        //     "-u-" = prefix for extensions registered by Unicode (RFC 6067)
        //     "kn" = natural sorting of numbers
        //     "true" = true
        // ref: https://peter.eisentraut.org/blog/2023/05/16/overview-of-icu-collation-settings
        session.QueueSqlCommand(
            $"CREATE COLLATION IF NOT EXISTS {QualifiedName} (provider = icu, locale = 'und-u-kn-true');"
        );
        await session.SaveChangesAsync(token);
    }
}
