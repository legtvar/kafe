namespace Kafe.Data;

public record Diagnostic(
    DiagnosticKind Kind,
    LocalizedString Message,
    string ValidationStage
);
