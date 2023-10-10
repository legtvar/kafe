using System;
using Npgsql.Replication;

namespace Kafe.Data;

[Flags]
public enum Permission
{
    /// <summary>
    /// No action can be done on the entity.
    /// </summary>
    None = 0,

    /// <summary>
    /// The entity can be viewed.
    /// </summary>
    Read = 1 << 0,

    /// <summary>
    /// The entity can be "appended to" (e.g., a project can be added to a project group).
    /// </summary>
    Append = 1 << 1,
    
    /// <summary>
    /// The contents of the entity can be viewed.
    /// </summary>
    Inspect = 1 << 2,

    /// <summary>
    /// The entity can be written to and archived.
    /// </summary>
    Write = 1 << 3,

    All = Read | Append | Inspect | Write
}
