using Kafe.Api.Diagnostics;

namespace Kafe.Api;

public class ApiMod : IMod
{
    public static string Moniker => "api";

    public void Configure(ModContext context)
    {
        context.AddDiagnosticPayload<BadCsvDiagnostic>();
    }
}
