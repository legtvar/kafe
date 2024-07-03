using System;

namespace Kafe.Data;

// NB: Must be int because PostgreSQL/Npgsql doesn't seem to have unsigned integers.
//     It probably could be fixed somehow but let's just say that the sign bit is reserved for future purposes.
[Flags]
public enum Permission : int
{
    /// <summary>
    /// No action can be done on the entity.
    /// </summary>
    None = 0,

    /// <summary>
    /// The entity can be viewed. Its metadata can be read. Is not inheritable.
    /// </summary>
    Read = 1 << 0,

    /// <summary>
    /// The entity can be "appended to" (e.g., a project can be added to a project group). Is not inheritable.
    /// </summary>
    Append = 1 << 1,

    /// <summary>
    /// The contents or children of the entity can be viewed. Is inheritable.
    /// </summary>
    /// <remarks>
    /// This right implies read on all descendant entities. For example:
    /// <list type="bullet">
    ///     <item>
    ///     Inspect on Organization implies Read and Inspect on all of its Playlists and ProjectGroups.
    ///     </item>
    ///     <item>
    ///     Inspect on ProjectGroups implies Read and Inspect on all of its Projects.
    ///     </item>
    ///     <item>
    ///     Inspect on Projects implies Read and Inspect on all of its Artifacts.
    ///     </item>
    ///     <item>
    ///     Inspect on Playlists implies Read and Inspect on all of its Artifacts.
    ///     </item>
    ///     <item>
    ///     Inspect on `system` implies Inspect on everything.
    ///     </item>
    /// </list>
    /// </remarks>
    Inspect = 1 << 2,

    /// <summary>
    /// The entity can be edited. Is inheritable.
    /// </summary>
    Write = 1 << 3,

    /// <summary>
    /// The entity can be reviewed---comments regarding it can be published and deliver to project owners.
    /// Project owners are all accounts with an explicit (not implied/inherited) Write permission.
    /// </summary>
    /// <remarks>
    /// Is inherited from Organization to ProjectGroup to Project, but right now it only makes sense on projects.
    /// </remarks>
    Review = 1 << 4,

    /// <summary>
    /// The entity can be administered. Account permissions to the entity can be given or taken. Is inheritable.
    /// </summary>
    Administer = 1 << 5,

    /// <summary>
    /// All permissions that are inheritable.
    /// </summary>
    // NB: This value is actually used to implement the inheritability. Tread carefully.
    Inheritable = Inspect | Write | Review | Administer,

    /// <summary>
    /// Future-proof omnipotency over an entity.
    /// </summary>
    All = int.MaxValue
}
