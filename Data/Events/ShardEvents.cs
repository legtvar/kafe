using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Events;

public interface IShardCreated
{
    Hrib ShardId { get; }

    CreationMethod CreationMethod { get; }

    Hrib ArtifactId { get; }
}
