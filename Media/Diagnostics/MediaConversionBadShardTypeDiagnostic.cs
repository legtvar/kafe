using System;

namespace Kafe.Media.Diagnostics;

public class MediaConversionBadShardTypeDiagnostic(
    Hrib ShardId,
    Type ShardPayloadType
) : IDiagnosticPayload
{
    public static string Moniker => "media-conversion-bad-shard-type";
    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Media Conversion of Unsupported Shard Type"),
        (Const.CzechCulture, "Konverze média ve střípku nepododporovaného typu")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "Cannot convert shard '{ShardId}' because media conversion of shard type '{ShardPayloadType}' is not supported."
        ),
        (
            Const.CzechCulture,
            "Střípek '{ShardId}' nelze zkonvertovat, jelikož konverze médií nepodporuje střípky typu '{ShardPayloadType}'."
        )
    );
}
