namespace Kafe.Media;

public record VariantShardLink : IShardLinkPayload
{
    public static string Moniker => "variant";

    public string? Preset { get; init; }
}
