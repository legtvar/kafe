using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Kafe.Api.Diagnostics;

/// <summary>
/// A diagnostic payload used for compatibility between ASP's default error reporting
/// through a <see cref="ProblemDetailsFactory"/>.
/// </summary>
public record AspProblemDiagnostic(
    int ProblemStatusCode,
    string? ProblemTitle = null,
    string? ProblemType = null,
    string? ProblemDetail = null,
    string? ProblemInstance = null
) : IDiagnosticPayload
{
    public static string Moniker => "asp-problem";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "ASP.NET Core Problem"),
        (Const.CzechCulture, "Problém v ASP.NET Core")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "An ASP.NET Core problem,'{ProblemTitle}', occurred. See the payload of this diagnostic for details."
        ),
        (
            Const.CzechCulture,
            "Nastal problém v ASP.NET Core typu '{ProblemTitle}'. Pro detaily nahlédněte do obsahu tohoto hlášení."
        )
    );
}
