using Marten.Events.CodeGeneration;
using System.Text.Json.Serialization;

namespace Kafe.Data.Aggregates;

public interface IEntity
{
    string Id { get; }

    [MartenIgnore, JsonIgnore]
    Hrib Hrib => Id;
}
