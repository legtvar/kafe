using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Kafe;

public static class LoggerExtensions
{
    public static void LogErr<T>(this ILogger self, Err<T> err, string? message, params object?[] args)
    {
        if (err.Diagnostic.IsValid)
        {
            self.LogDiagnostic(err.Diagnostic, message, args);
        }
    }

    public static void LogDiagnostic(this ILogger self, Diagnostic diagnostic, string? message, params object?[] args)
    {
        var level = diagnostic.Severity switch
        {
            DiagnosticSeverity.Debug => LogLevel.Debug,
            DiagnosticSeverity.Info => LogLevel.Information,
            DiagnosticSeverity.Warning => LogLevel.Warning,
            DiagnosticSeverity.Error => LogLevel.Error,
            _ => LogLevel.None
        };

        self.Log(level, message + "\n{Diagnostic}", [..args, diagnostic.ToString(CultureInfo.CurrentCulture)]);
    }
}
