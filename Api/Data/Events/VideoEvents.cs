using Kafe.Media;

namespace Kafe.Data.Events;

public record VideoCreated(
    CreationMethod CreationMethod,
    string? FileName,
    MediaInfo Metadata);
