namespace Kafe;

public interface IMergeable
{
    /// <summary>
    /// Merge an object of this mergeable type wrapped in an <see cref="KafeObject"/> as <paramref name="self"/> with
    /// another object of potentitally completely different type.
    /// </summary>
    static abstract KafeObject? Merge(KafeObject self, KafeObject @new);
}
