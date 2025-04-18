namespace Kafe.Data.Aggregates;

public interface IEntity
{
    /// <summary>
    /// A human-readable name of the entity type.
    /// </summary>
    public virtual static LocalizedString? Name { get; }

    [Hrib] string Id { get; }
}
