using System;

namespace Kafe;

public interface IMod
{
    /// <summary>
    /// A short name for the mod (e.g., core, media, etc.).
    /// </summary>
    /// 
    /// <remarks>
    /// May contain only lower-case letters, numbers, or '-'.
    /// </remarks>
    // NB: Is declared as virtual so that this interface can be used in collection types (see CS8920).
    public virtual static string Name
        => throw new InvalidOperationException("The IMod interface is not an instance "
            + "of a mod and thus does not have a name. Use a concrete mod type instead.");

    public void Configure(ModContext context);
}
