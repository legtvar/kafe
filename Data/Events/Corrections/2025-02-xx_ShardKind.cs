using System;

namespace Kafe.Data
{
    [Obsolete("Use the new shard universal shard events instead.")]
    public enum ShardKind
    {
        Unknown = 0,
        Video = 1,
        Image = 2,
        Subtitles = 3,
        Blend = 4
    }
}
