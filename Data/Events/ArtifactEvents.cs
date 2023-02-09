namespace Kafe.Data.Events;

public record ArtifactCreated(
    CreationMethod CreationMethod,
    LocalizedString Name,
    string ProjectId);

public record ArtifactInfoChanged(
    LocalizedString? Name,
    string? ProjectId);

public record ArtifactShardAdded(string ShardId);

public record ArtifactShardRemoved(string ShardId);
