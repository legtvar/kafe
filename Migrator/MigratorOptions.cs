namespace Kafe.Migrator;

public record MigratorOptions(
    string? WmaVideosDirectory,
    string? KafeVideosDirectory,
    string OrganizationId);
