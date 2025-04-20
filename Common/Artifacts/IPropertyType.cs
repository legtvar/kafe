namespace Kafe;

public interface IPropertyType
{
    public static virtual string? Moniker { get; }

    public static virtual LocalizedString? Title { get; }
}
