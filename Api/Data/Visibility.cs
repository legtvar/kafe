namespace Kafe.Data;

public enum Visibility
{
    Unknown, // admin-only
    Private, // people with the capability / link
    Internal, // all people with access to KAFE
    Public // all of the Internet
}
