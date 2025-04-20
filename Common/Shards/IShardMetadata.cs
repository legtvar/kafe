using System;

namespace Kafe;

public interface IShardMetadata
{
    static virtual string? Moniker { get; }

    public static virtual LocalizedString? Title { get; }
}
