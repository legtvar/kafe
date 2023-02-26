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
    Hrib ShardId { get; }
}

public interface IShardCreated : IShardEvent
{
    CreationMethod CreationMethod { get; }

    Hrib ArtifactId { get; }
}

public interface IShardVariantsAdded : IShardEvent
{
    IEnumerable<string> GetVariantNames();
}

public interface IShardVariantsRemoved : IShardEvent
{
    IEnumerable<string> GetVariantNames();
}
