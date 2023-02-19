namespace Kafe.Migrator;

public record ArtifactMigrationInfo(
    int WmaId,
    string ArtifactId,
    string VideoShardId,
    string Name);
