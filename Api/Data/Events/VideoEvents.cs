using Kafe.Media;

namespace Kafe.Data.Events;

public record VideoCreated(
    CreationMethod CreationMethod,
    LocalizedString Name,
    string? FileName,
    MediaInfo Metadata);

public record VideoInfoChanged(
    LocalizedString? Name,
    string? FileName,
    MediaInfo? Metadata);
