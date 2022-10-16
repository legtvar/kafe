namespace Kafe.Data;

public enum ProjectVisibility
{
    Unknown, // admin-only
    Private, // people with the capability / link
    Internal, // all people with access to KAFE
    Public // all of the Internet
}
