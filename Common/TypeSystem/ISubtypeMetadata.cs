using System;

namespace Kafe;

public interface ISubtypeMetadata
{
    public KafeType KafeType { get; }

    public Type DotnetType { get; }
}
