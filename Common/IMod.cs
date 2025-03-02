namespace Kafe;

public interface IMod
{
    /// <summary>
    /// A short name for the mod (e.g., core, media, etc.).
    /// </summary>
    /// <remarks>
    /// May contain only lower-case letters, numbers, or '-'.
    /// This property will be read before <see cref="Configure"/> is invoked.
    /// Everything registered through <see cref="Configure"/> will be scoped to this namespace.
    /// </remarks>
    string Slug { get; }

    void Configure(ModContext context)
    {

    }
}
