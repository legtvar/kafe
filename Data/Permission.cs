using System;

namespace Kafe.Data;

// NB: must be int because postgres, doesn't seem to have unsigned integers
[Flags]
public enum Permission : int
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
    /// <remarks>
    /// Currently has meaning only on project groups where it represents the ability to read all of the
    /// group's projects.
    /// </remarks>
    Inspect = 1 << 2,

    /// <summary>
    /// The entity can be written to and archived.
    /// </summary>
    Write = 1 << 3,

    /// <summary>
    /// The entity can be reviewed (comments regarding it can be published and deliver to project owners).
    /// </summary>
    Review = 1 << 4,

    All = int.MaxValue
}
