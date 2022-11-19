namespace Kafe.Data.Events;

public record VideoConversionCreated(
    string VideoId
);
public record VideoConversionCompleted;
public record VideoConversionFailed(
    string Reason
);
