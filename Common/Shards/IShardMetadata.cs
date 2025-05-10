using System;

namespace Kafe;

public interface IShardMetadata
{
    public static virtual string? Moniker { get; }

    public static virtual LocalizedString? Title { get; }
}
