using Npgsql.Replication;

namespace Kafe.Data;

public enum Permission
{
    /// <summary>
    /// No action can be done on the entity.
    /// </summary>
    None = 0,

    /// <summary>
    /// The entity can be viewed.
    /// </summary>
    Read = 1,

    /// <summary>
    /// The entity can be "appended to" (e.g., a project can be added to a project group). Implies <see cref="Read"/>.
    /// </summary>
    Append = 2,

    /// <summary>
    /// The entity can be written and overriden. Implies <see cref="Append"/>.
    /// </summary>
    Write = 3
}
