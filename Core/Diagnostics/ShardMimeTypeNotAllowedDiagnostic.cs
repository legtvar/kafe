using System.Collections.Immutable;

namespace Kafe.Core.Diagnostics;

public record ShardMimeTypeNotAllowedDiagnostic(
    Hrib ShardId,
    LocalizedString ShardName,
    string MimeType,
    ImmutableArray<string> AllowedMimeTypes
) : IDiagnosticPayload
{
    public static string Moniker => "shard-mime-type-not-allowed";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Shard MIME Type Not Allowed"),
        (Const.CzechCulture, "Střípek má nepovolený MIME typ")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "Shard '{ShardName}' has the '{MimeType}' MIME type. This type is not allowed. "
                + "Use one of the following MIME types instead: {AllowedMimeTypes}."
        ),
        (
            Const.CzechCulture,
            "Střípek '{ShardName}' má zakázaný MIME typ '{MimeType}'. "
                + "Použijte místo něj jeden z následujících MIME typů: {AllowedMimeTypes}."
        )
    );
}
