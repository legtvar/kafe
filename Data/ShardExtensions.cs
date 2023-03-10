using Kafe.Data.Aggregates;
using Kafe.Data.Events;
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

    public static ShardKind GetShardKind(this IShardEvent e)
    {
        return e switch
        {
            IVideoShardEvent => ShardKind.Video,
            IImageShardEvent => ShardKind.Image,
            ISubtitlesShardEvent => ShardKind.Subtitles,
            _ => ShardKind.Unknown
        };
    }

}
