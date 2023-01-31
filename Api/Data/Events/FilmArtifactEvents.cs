using Kafe.Media;

namespace Kafe.Data.Events;

public record FilmArtifactCreated(
    CreationMethod CreationMethod,
    LocalizedString Name);

public record FilmArtifactMediaAdded(
    MediaInfo Media);

public record FilmArtifactMediaRemoved(
    string Path);

public record ImageArtifactCreated(
    CreationMethod CreationMethod,
    LocalizedString Name);

public record ArtifactInfoChanged(
    LocalizedString? Name);
