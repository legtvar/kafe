namespace Kafe.Migrator;

public record VideoShardMigrationInfo(
    int WmaId,
    string ArtifactId,
    string VideoShardId,
    string Name,
    DateTimeOffset? AddedOn);
