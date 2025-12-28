namespace Kafe;

public interface IShardLinkMetadata
{
    /// <summary>
    /// A short name for the shard link type.
    /// </summary>
    ///
    /// <remarks>
    /// May contain only lower-case letters, numbers, or '-'.
    /// </remarks>
    public static virtual string? Moniker { get; }

    public static virtual LocalizedString? Title { get; }
}
