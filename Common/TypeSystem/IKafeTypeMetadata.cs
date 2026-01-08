namespace Kafe;

public interface IKafeTypeMetadata
{
    /// <summary>
    /// Short identifier used when registering this type in <see cref="ModContext"/>.
    /// </summary>
    public static virtual string? Moniker { get; }

    /// <summary>
    /// A human-readable, localized name for this type.
    /// </summary>
    public static virtual LocalizedString? Title { get; }
}
