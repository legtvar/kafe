using Marten.Events.CodeGeneration;
using System.Text.Json.Serialization;

namespace Kafe.Data.Aggregates;

public interface IEntity
{
    string Id { get; }

    [MartenIgnore, JsonIgnore]
    public Hrib Hrib => Id;
}

public abstract record EntityBase : IEntity
{
    public string Id => Hrib;

    [MartenIgnore, JsonIgnore]
    public Hrib Hrib { get; init; }

    public EntityBase(string id)
    {
        Hrib = id;
    }

    public EntityBase(Hrib hrib)
    {
        Hrib = hrib;
    }

    public EntityBase()
    {
        Hrib = Hrib.Invalid;
    }
}
