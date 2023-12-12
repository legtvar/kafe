namespace Kafe.Data.Aggregates;

public interface IShardEntity : IEntity
{
    ShardKind Kind { get; }
    
    [Hrib]
    string ArtifactId { get; }
}
