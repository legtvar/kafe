using Microsoft.Extensions.Logging;

namespace Kafe;

public static class LoggerExtensions
{
    public static void LogError<T>(this ILogger self, Err<T> err, string? message, params object?[] args)
    {
        self.LogError(message + "\n{Errors}", [..args, err.Errors]);
    }

    public static void LogError(this ILogger self, Error error, string? message, params object?[] args)
    {
        self.LogError(new Err<bool>(error), message, args);
    }
}
