namespace Kafe.Data.Aggregates;

public interface IVisibleEntity : IEntity
{
    Visibility Visibility { get; }
}
