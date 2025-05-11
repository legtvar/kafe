namespace Kafe.Media;

public record VariantLinkMetadata : IShardLinkMetadata
{
    public static string Moniker { get; } = "variant";

    public string? Preset { get; init; }
}
