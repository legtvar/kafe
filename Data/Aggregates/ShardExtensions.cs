using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Aggregates;

public static class ShardExtensions
{
    public static ImmutableArray<string> GetVariants(this Shard shard)
    {
        return shard switch
        {
            VideoShard v => v.Variants.Select(v => v.ToString()).ToImmutableArray(),
            ImageShard i => i.Variants.Select(v => v.ToString()).ToImmutableArray(),
            SubtitlesShard s => s.Variants,
            _ => throw new NotSupportedException($"Shard of type '{shard.GetType()}' is not supported by this method."),
        };
    }
}
