namespace Kafe.Data;

public enum Visibility
{
    /// <summary>
    /// Admin-only.
    /// </summary>
    Unknown,

    /// <summary>
    /// People with ownership / link.
    /// </summary>
    Private,

    /// <summary>
    /// All people with internal access to Kafe.
    /// </summary>
    Internal,

    /// <summary>
    /// All of the Internet.
    /// </summary>
    Public
}
