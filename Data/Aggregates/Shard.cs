﻿using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Kafe.Data.Aggregates;

public abstract record Shard(
    string Id,
    CreationMethod CreationMethod,
    string ArtifactId) : IEntity
{
    public abstract ShardKind Kind { get; }
}
