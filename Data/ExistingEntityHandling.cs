namespace Kafe.Data;

public enum ExistingEntityHandling
{
    /// <summary>
    /// If the entity exists, <see cref="Update"/> it. If it doesn't exist, <see cref="Insert"/> a new one.
    /// </summary>
    Upsert,

    /// <summary>
    /// Require that the entity DOES NOT exist, so that a brand new one can be created.
    /// </summary>
    Insert,

    /// <summary>
    /// Require that the DOES already exist, so that it can be updated;
    /// </summary>
    Update,
}
