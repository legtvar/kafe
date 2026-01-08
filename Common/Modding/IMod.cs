using System;

namespace Kafe;

public interface IMod
{
    /// <summary>
    /// A short, dash-case name for the mod (e.g., core, media, etc.).
    /// </summary>
    // NB: Is declared as virtual so that this interface can be used in collection types (see CS8920).
    public static virtual string Moniker => throw new InvalidOperationException($"The {nameof(IMod)} interface "
        + $"is not a concrete type and thus does not have a {nameof(Moniker)}.");

    public void ConfigureOptions(KafeBrewingOptions options) { }

    public void Configure(ModContext context);

    public void PostConfigure() { }
}
