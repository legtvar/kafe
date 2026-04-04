using System.Collections.Immutable;

namespace Kafe.Api.Diagnostics;

/// <summary>
/// A <see cref="Diagnostic"/>-land equivalent of a HTTP 400 (i.e., the client messed up the request).
/// </summary>
/// <param name="Inner">
/// The reasons for the failure.
/// Typically, diagnostics with payloads of type <see cref="ParameterDiagnostic"/>.
/// </param>
public record BadRequestDiagnostic(
    ImmutableArray<Diagnostic> Inner
) : IDiagnosticPayload
{
    public static string Moniker => "bad-request";

    public static DiagnosticSeverity DefaultSeverity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Bad Request"),
        (Const.CzechCulture, "Chybný dotaz")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, $"The request contains errors. See '{nameof(Inner)}' for details."),
        (Const.CzechCulture, $"Dotaz obsahuje chyby. Pro podrobnosti nahlédněte do '{nameof(Inner)}'.")
    );
}
