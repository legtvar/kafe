namespace Kafe.Core;

public record BlobShard : IShardPayload
{
    public static string Moniker => "blob";

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Binary blob"),
        (Const.CzechCulture, "Binární blob")
    );
}
