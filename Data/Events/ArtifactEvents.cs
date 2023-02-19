namespace Kafe.Data.Events;

public record ArtifactCreated(
    Hrib ArtifactId,
    CreationMethod CreationMethod,
    LocalizedString Name);

public record ArtifactInfoChanged(
    Hrib ArtifactId,
    LocalizedString? Name);
