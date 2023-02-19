namespace Kafe.Data.Events;

public record VideoConversionCreated(
    Hrib ConversionId,
    string VideoId
);

public record VideoConversionCompleted(
    Hrib ConversionId
);

public record VideoConversionFailed(
    Hrib ConversionId,
    LocalizedString Reason
);
