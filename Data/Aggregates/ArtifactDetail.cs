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
    [Hrib] string Id,
    CreationMethod CreationMethod,
    [LocalizedString] ImmutableDictionary<string, string> Name,
    DateTimeOffset AddedOn,
    ImmutableArray<ArtifactShardInfo> Shards,
    [KafeType(typeof(ImmutableArray<Hrib>))] ImmutableArray<string> ContainingProjectIds
);

public record ArtifactShardInfo(
    [Hrib] string ShardId,
    ShardKind Kind,
    ImmutableHashSet<string> Variants
);

public class ArtifactDetailProjection : MultiStreamAggregation<ArtifactDetail, string>
{
    public ArtifactDetailProjection()
    {
        Identity<ArtifactCreated>(a => a.ArtifactId);
        Identity<IShardCreated>(c => c.ArtifactId);
        Identity<ProjectArtifactAdded>(c => c.ArtifactId);
        Identity<ProjectArtifactRemoved>(c => c.ArtifactId);
        IncludeType<IShardEvent>();
        CustomGrouping(new Grouper());
    }

    public ArtifactDetail Create(ArtifactCreated e)
    {
        return new ArtifactDetail(
            Id: e.ArtifactId,
            CreationMethod: e.CreationMethod,
            Name: e.Name,
            AddedOn: e.AddedOn,
            Shards: ImmutableArray<ArtifactShardInfo>.Empty,
            ContainingProjectIds: ImmutableArray<string>.Empty
        );
    }

    public ArtifactDetail Apply(IShardCreated e, ArtifactDetail a)
    {
        return a with
        {
            Shards = a.Shards.Add(new ArtifactShardInfo(
                ShardId: e.ShardId,
                Kind: e.GetShardKind(),
                Variants: ImmutableHashSet.Create(Const.OriginalShardVariant)))
        };
    }

    public ArtifactDetail Apply(IShardVariantAdded e, ArtifactDetail a)
    {
        var oldShard = a.Shards.Single(s => s.ShardId == e.ShardId);
        return a with
        {
            Shards = a.Shards.Replace(oldShard, oldShard with
            {
                Variants = oldShard.Variants.Add(e.Name)
            })
        };
    }

    public ArtifactDetail Apply(IShardVariantRemoved e, ArtifactDetail a)
    {
        var oldShard = a.Shards.Single(s => s.ShardId == e.ShardId);
        return a with
        {
            Shards = a.Shards.Replace(oldShard, oldShard with
            {
                Variants = oldShard.Variants.Remove(e.Name)
            })
        };
    }

    public ArtifactDetail Apply(ProjectArtifactAdded e, ArtifactDetail a)
    {
        return a with
        {
            ContainingProjectIds = a.ContainingProjectIds.Add(e.ProjectId)
        };
    }

    public ArtifactDetail Apply(ProjectArtifactRemoved e, ArtifactDetail a)
    {
        return a with
        {
            ContainingProjectIds = a.ContainingProjectIds.RemoveAll(p => p == e.ProjectId)
        };
    }

    private class Grouper : IAggregateGrouper<string>
    {
        public async Task Group(IQuerySession session, IEnumerable<IEvent> events, ITenantSliceGroup<string> grouping)
        {
            var filteredEvents = events.Where(e => e.EventType.IsAssignableTo(typeof(IShardVariantAdded))
                || e.EventType.IsAssignableTo(typeof(IShardVariantRemoved)))
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
