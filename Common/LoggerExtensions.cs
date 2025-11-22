using Microsoft.Extensions.Logging;

namespace Kafe;

public static class LoggerExtensions
{
    public static void LogError<T>(this ILogger self, Err<T> err, string? message, params object?[] args)
    {
        self.LogError(message + "\n{Errors}", [..args, err.Errors]);
    }
}
