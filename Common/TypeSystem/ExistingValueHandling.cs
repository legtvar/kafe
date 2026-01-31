namespace Kafe;

public enum ExistingValueHandling
{
    /// <summary>
    /// Always set the new value, overwriting any previously existing value.
    /// </summary>
    OverwriteExisting,

    /// <summary>
    /// Only set the new value if there is no existing value.
    /// </summary>
    /// <remarks>
    /// Can be used to provide a fallback value.
    /// </remarks>
    KeepExisting,

    /// <summary>
    /// Attempt to merge the two values. If merge fails, then <see cref="OverwriteExisting"/>.
    /// </summary>
    /// <remarks>
    /// May cause:
    ///     <list type="bullet">
    ///         <item>Items to be appended or prepended to arrays.</item>
    ///         <item>Arrays to be concatenated, with existing items first, new items later.</item>
    ///         <item>Invocation of <see cref="IMergeable{T}.MergeWith(T)"/>.</item>
    ///     </list>
    /// </remarks>
    MergeOrOverwrite,

    /// <summary>
    /// Attempt to merge the two values. If merge fails, then <see cref="KeepExisting"/>.
    /// </summary>
    /// <remarks>
    /// Is the same <see cref="MergeOrOverwrite"/> except it falls back to <see cref="KeepExisting" />.
    /// </remarks>
    MergeOrKeep
}
