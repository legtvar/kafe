namespace Kafe.Core;

public record ArchiveShard : IShardPayload
{
    public static string Moniker => "archive";

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Compressed archive"),
        (Const.CzechCulture, "Zkomprimovaný archiv")
    );

    public ArchiveKind ArchiveKind { get; init; }
}
