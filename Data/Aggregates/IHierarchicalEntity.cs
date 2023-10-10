namespace Kafe.Data.Aggregates;

public interface IHierarchicalEntity : IEntity
{
    public string ParentId { get; }
}
