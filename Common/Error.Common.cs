namespace Kafe.Common;

public readonly partial record struct Error
{
    public const string NotFoundId = nameof(NotFound);
    public const string BadHribId = nameof(BadHrib);

    public static Error NotFound(string? message = null)
    {
        return new Error(NotFoundId, message ?? "Could not be found.");
    }
    
    public static Error NotFound(Hrib id, string description = "An entity")
    {
        return new Error(NotFoundId, $"{description} with identifier '{id}' could not be found.");
    }
    
    public static Error BadHrib(string? value = null)
    {
        value = string.IsNullOrEmpty(value) ? "null" : $"'{value}'";
        return new Error(BadHribId, $"String {value} is not a valid identifier.");
    }
}
