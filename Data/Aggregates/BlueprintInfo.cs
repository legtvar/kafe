namespace Kafe.Data.Aggregates;

// TODO: Blueprints.
public record BlueprintInfo(
    [Hrib] string Id

    ) : IEntity
{
    Hrib IEntity.Id => Id;
}
