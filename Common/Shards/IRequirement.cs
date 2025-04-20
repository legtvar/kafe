using System;

namespace Kafe;

public interface IRequirement
{
    /// <summary>
    /// A short name for the requirement.
    /// </summary>
    /// 
    /// <remarks>
    /// May contain only lower-case letters, numbers, or '-'.
    /// </remarks>
    // NB: Is declared as virtual so that this interface can be used in collection types (see CS8920).
    public virtual static string? Moniker { get; }

    public static virtual LocalizedString? Title { get; }

    public virtual static KafeTypeAccessibility Accessibility { get; } = KafeTypeAccessibility.Public;
}
