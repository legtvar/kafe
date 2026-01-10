namespace Kafe.Media;

public record VariantLinkMetadata : IShardLinkMetadata
{
    public static string Moniker => "variant";

    public string? Preset { get; init; }
}
