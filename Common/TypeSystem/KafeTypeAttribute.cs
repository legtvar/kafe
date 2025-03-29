using System;

namespace Kafe;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class KafeTypeAttribute : Attribute
{
    public KafeTypeAttribute(Type type)
    {
        Type = type;
    }
    public Type Type { get; }
}
