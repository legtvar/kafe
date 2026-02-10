namespace Kafe.Mate.Diagnostics;

public record PigeonsFailedToRunDiagnostic(
    string Error
) : IDiagnosticPayload
{
    public static string Moniker => "pigeons-failed-to-run";

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "PIGEOnS Failed to Run"),
        (Const.CzechCulture, "PIGEOnS selhalo při spuštění")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "PIGEOnS test could not be run: {Error}"),
        (Const.CzechCulture, "PIGEOnS test nemohl být spuštěn: {Error}")
    );
}
