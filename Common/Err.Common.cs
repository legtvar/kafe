namespace Kafe.Common;

public static class Err
{
    public const string NotFoundId = nameof(NotFound);

    public static Err<T> NotFound<T>(string? message = null)
    {
        return new Error(NotFoundId, message ?? "Could not be found.");
    }
    
    public static Err<T> NotFound<T>(Hrib id, string description = "An entity")
    {
        return new Error(NotFoundId, $"{description} with id '{id}' could not be found.");
    }
}
