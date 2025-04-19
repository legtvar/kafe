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
    public virtual static string Name
        => throw new InvalidOperationException($"The {nameof(IRequirement)} interface is not an instance "
            + "of a requirement and thus does not have a name. Use a concrete mod type instead.");

    public virtual static KafeTypeAccessibility Accessibility { get; } = KafeTypeAccessibility.Public;
}
