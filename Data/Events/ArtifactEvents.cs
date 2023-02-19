namespace Kafe.Data.Events;

public record ArtifactCreated(
    CreationMethod CreationMethod,
    LocalizedString Name);

public record ArtifactInfoChanged(
    LocalizedString? Name);
