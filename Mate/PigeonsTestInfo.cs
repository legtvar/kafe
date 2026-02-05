namespace Kafe.Mate;

public record PigeonsTestInfo(
    string? Label,
    string? State,
    string? Datablock,
    string? Message,
    string? Traceback
);

public record PigeonsTestInfoJsonFormat
{
    public string? Label { get; set; }
    public string? State { get; set; }
    public string? Datablock { get; set; }
    public string? Message { get; set; }
    public string? Traceback { get; set; }

    public PigeonsTestInfo ToPigeonsTestInfo()
    {
        return new PigeonsTestInfo(Label, State, Datablock, Message, Traceback);
    }
}

public record PigeonsTestRequest(
    [Hrib]
    string ShardId,
    string HomeworkType,
    string Path
);
