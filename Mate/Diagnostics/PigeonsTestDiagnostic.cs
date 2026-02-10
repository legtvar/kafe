namespace Kafe.Mate.Diagnostics;

public record PigeonsTestDiagnostic(
    string? Label,
    string? Datablock,
    string? InnerMessage,
    string? Traceback
) : IDiagnosticPayload
{
    public static string Moniker => "pigeons-test";

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "PIGEOnS Test Diagnostic"),
        (Const.CzechCulture, "Hlášení z PIGEOnS testu")
    );

    public static LocalizedString FallbackInnerMessage { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "An unknown event occurred."),
        (Const.CzechCulture, "Došlo k neznámé události.")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "{Label:- {}:|}{Datablock: [{}]|}{InnerMessage: {}|" + FallbackInnerMessage[Const.InvariantCulture]
            + "}{Traceback: Traceback: {}|}"
        ),
        (
            Const.CzechCulture,
            "{Label:- {}:|}{Datablock: [{}]|}{InnerMessage: {}|" + FallbackInnerMessage[Const.CzechCulture]
            + "}{Traceback: Traceback: {}|}"
        )
    );

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Info;
}
