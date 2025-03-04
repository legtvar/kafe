namespace Kafe;

public enum KafeTypeUsage
{
    None = 0,
    ArtifactProperty = 1 << 0,
    ShardMetadata = 1 << 1,
    Requirement = 1 << 2,
}
