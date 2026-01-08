namespace Kafe;

public interface IKafeTypeMetadata
{
    /// <summary>
    /// Short identifier used when registering this type in <see cref="ModContext"/>.
    /// </summary>
    public static virtual string? Moniker => null;

    /// <summary>
    /// A human-readable, localized name for this type.
    /// </summary>
    public static virtual LocalizedString? Title => null;

    public static string? GetMoniker<T>()
        where T : IKafeTypeMetadata
    {
        return T.Moniker;
    }

    public static LocalizedString? GetTitle<T>()
        where T : IKafeTypeMetadata
    {
        return T.Title;
    }
}
