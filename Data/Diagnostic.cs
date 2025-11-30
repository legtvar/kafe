using System.Collections.Generic;

namespace Kafe.Data;

public record Diagnostic(
    DiagnosticKind Kind,
    LocalizedString Message,
    string ValidationStage
)
{
    public Diagnostic Format(params IEnumerable<object> args)
    {
        return this with
        {
            Message = LocalizedString.Format(Message, args)
        };
    }
}
