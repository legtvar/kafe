using Kafe.Media;

namespace Kafe.Data.Events;

public record ArtifactCreated(
    CreationMethod CreationMethod,
    ArtifactKind Kind,
    LocalizedString Name,
    string? FileName,
    MediaInfo Metadata);

public record ArtifactInfoChanged(
    LocalizedString? Name,
    string? FileName,
    MediaInfo? Metadata);
