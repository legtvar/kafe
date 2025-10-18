namespace Pigeons.Services;

public record PigeonsTestInfo(
    string? Label,
    string? State,
    string? Datablock,
    string? Message,
    string? Traceback
)
{ }

public class PigeonsTestResultsSerializable : Dictionary<int, PigeonsTestResultSerializable>
{
}

public class PigeonsTestResultSerializable
{
    public string? label { get; set; }
    public string? state { get; set; }
    public string? datablock { get; set; }
    public string? message { get; set; }
    public string? traceback { get; set; }
};