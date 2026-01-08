using System;

namespace Kafe;

public interface IShardPayload : IKafeTypeMetadata
{
    public static readonly string TypeCategory = "shard";
}
