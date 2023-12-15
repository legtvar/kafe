namespace Kafe.Data.Aggregates;

public interface IVisibleEntity : IEntity
{
    Permission GlobalPermissions { get; }
}
