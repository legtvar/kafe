using System;

namespace Kafe;

public interface IShardPayload : IKafeTypeMetadata
{
    public static readonly string TypeCategory = "shard";

    public static virtual string? Moniker { get; }

    public static virtual LocalizedString? Title { get; }
}
