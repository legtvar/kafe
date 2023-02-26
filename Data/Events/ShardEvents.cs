using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
