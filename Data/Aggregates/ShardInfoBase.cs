using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Kafe.Data.Aggregates;

public abstract record ShardInfoBase(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    [Hrib] string ArtifactId,
    DateTimeOffset CreatedAt) : IEntity
{
    public abstract ShardKind Kind { get; }
}
