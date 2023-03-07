using Marten.Events.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Kafe.Data.Events;

public interface IShardEvent
{
    [Hrib]
    string ShardId { get; }
}

public interface IShardCreated : IShardEvent
{
    CreationMethod CreationMethod { get; }

    [Hrib]
    string ArtifactId { get; }
}

public interface IShardVariantAdded : IShardEvent
{
    string Name { get; }
}

public interface IShardVariantRemoved : IShardEvent
{
    string Name { get; }
}
