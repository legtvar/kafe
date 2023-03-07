using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record VideoConversionCreated(
    [Hrib] string ConversionId,
    string VideoId
);

public record VideoConversionCompleted(
    [Hrib] string ConversionId
);

public record VideoConversionFailed(
    [Hrib] string ConversionId,
    [LocalizedString] ImmutableDictionary<string, string> Reason
);
