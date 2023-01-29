using Kafe.Media;

namespace Kafe.Data.Aggregates;

public record Video(
    string Id,
    CreationMethod CreationMethod,
    string? FileName,
    VideoInfo Metadata);
