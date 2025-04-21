using System.Collections.Immutable;

namespace Kafe.Core.Diagnostics;

public record ShardMimeTypeNotAllowedDiagnostic(
    Hrib ShardId,
    LocalizedString ShardName,
    string MimeType,
    ImmutableArray<string> AllowedMimeTypes
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "shard-mime-type-not-allowed";

    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Shard MIME Type Not Allowed"),
        (Const.CzechCulture, "Střípek má nepovolený MIME typ")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
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
