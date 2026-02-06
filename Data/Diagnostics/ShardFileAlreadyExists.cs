using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Diagnostics;

public record ShardFileAlreadyExists(
    Hrib ShardId
) : IDiagnosticPayload
{
    public static string Moniker => "shard-file-already-exists";

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Shard File Already Exists"),
        (Const.CzechCulture, "Soubor střípku již existuje")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "Could not store a file for shard '{ShardId}' because it already has one. Shards cannot be overwritten."
        ),
        (
            Const.CzechCulture,
            "Soubor střípku '{ShardId}' nebylo možné uložit, jelikož již soubor má. Střípky nelze přepisovat."
        )
    );
}
