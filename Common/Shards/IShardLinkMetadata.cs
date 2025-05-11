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
    public virtual static string? Moniker { get; }
}
