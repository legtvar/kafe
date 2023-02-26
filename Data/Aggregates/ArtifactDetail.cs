using Kafe.Data.Events;
using Kafe.Media;
using Marten;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Aggregates;

public record ArtifactDetail(
    string Id,
    CreationMethod CreationMethod,
    LocalizedString Name,
    ImmutableArray<ArtifactShardInfo> Shards
);

public record ArtifactShardInfo(
    string ShardId,
    ShardKind Kind,
    ImmutableArray<string> Variants
);

public class ArtifactDetailProjection : MultiStreamAggregation<ArtifactDetail, string>
{
    public ArtifactDetailProjection()
    {
        Identity<ArtifactCreated>(a => a.ArtifactId);
        Identity<IShardCreated>(c => c.ArtifactId);
        IncludeType<IShardEvent>();
        CustomGrouping(new Grouper());
    }

    public ArtifactDetail Create(ArtifactCreated e)
    {
        return new ArtifactDetail(
            Id: e.ArtifactId,
            CreationMethod: e.CreationMethod,
            Name: e.Name,
            Shards: ImmutableArray<ArtifactShardInfo>.Empty);
    }

    public ArtifactDetail Apply(IShardCreated e, ArtifactDetail a)
    {
        return a with
        {
            Shards = a.Shards.Add(new ArtifactShardInfo(
                ShardId: e.ShardId,
                Kind: e.GetShardKind(),
                Variants: ImmutableArray.Create(Const.OriginalShardVariant)))
        };
    }

    public ArtifactDetail Apply(IShardVariantsAdded e, ArtifactDetail a)
    {
        var oldShard = a.Shards.Single(s => s.ShardId == e.ShardId);
        return a with
        {
            Shards = a.Shards.Replace(oldShard, oldShard with
            {
                Variants = oldShard.Variants.AddRange(e.GetVariantNames())
            })
        };
    }

    public ArtifactDetail Apply(IShardVariantsRemoved e, ArtifactDetail a)
    {
        var oldShard = a.Shards.Single(s => s.ShardId == e.ShardId);
        return a with
        {
            Shards = a.Shards.Replace(oldShard, oldShard with
            {
                Variants = oldShard.Variants.RemoveRange(e.GetVariantNames())
            })
        };
    }

    private class Grouper : IAggregateGrouper<string>
    {
        public async Task Group(IQuerySession session, IEnumerable<IEvent> events, ITenantSliceGroup<string> grouping)
        {
            var filteredEvents = events.Where(e => e.EventType.IsAssignableTo(typeof(IShardVariantsAdded))
                || e.EventType.IsAssignableTo(typeof(IShardVariantsRemoved)))
                .ToList();
            if (!filteredEvents.Any())
            {
                return;
            }

            var shardIds = filteredEvents.Select(e => e.StreamKey).ToList();

            var artifactShardPairs = await session.Events.QueryRawEventDataOnly<IShardCreated>()
                .Where(e => shardIds.Contains(e.ShardId))
                .Select(e => new { e.ArtifactId, e.ShardId })
                .ToListAsync();

            foreach (var group in artifactShardPairs
                .Select(p => new { p.ArtifactId, Events = filteredEvents.Where(e => e.StreamKey == p.ShardId) }))
            {
                grouping.AddEvents(group.ArtifactId, group.Events);
            }
        }
    }
}
