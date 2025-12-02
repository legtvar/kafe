namespace Kafe.Pigeons.Services;

public record BlenderProcessOutput(
    bool Success,
    string Message = ""
)
{ }
