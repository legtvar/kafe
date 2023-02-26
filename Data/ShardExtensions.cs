using Kafe.Data.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data;

public static class ShardExtensions
{
    public static Type ToAggregateType(this ShardKind value)
    {
        return value switch
        {
            ShardKind.Video => typeof(VideoShardInfo),
            ShardKind.Image => typeof(ImageShardInfo),
            ShardKind.Subtitles => typeof(SubtitlesShardInfo),
            _ => throw new NotSupportedException($"ShardKind '{value}' does not have an aggregate type assigned by " +
                "this method. This could be an oversight.")
        };
    }
}
