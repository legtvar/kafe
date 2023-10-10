namespace Kafe.Api.Endpoints;

public static class EndpointPolicy
{
    public const string AdministratorOnly = "AdministratorOnly";
    public const string Read = "Read";
    public const string Write = "Write";
    public const string Append = "Append";
    public const string Inspect = "Inspect";
    public const string ReadInspect = "ReadInspect";
}
