namespace Kafe;

public interface IScalar
{
    /// <summary>
    /// A short, dash-case name of the scalar type. If set, becomes <see cref="KafeType.Secondary"/>.
    /// </summary>
    public static virtual string? Moniker { get; }

    /// <summary>
    /// A human-readable name of the scalar type.
    /// </summary>
    public static virtual LocalizedString? Title { get; }
}
