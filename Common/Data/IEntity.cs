namespace Kafe;

public interface IEntity
{
    /// <summary>
    /// A human-readable name of the entity type.
    /// </summary>
    public virtual static LocalizedString? Name { get; }

    // NB: For the time being, Id is string despite always being a Hrib because of Marten
    [Hrib] string Id { get; }
}
