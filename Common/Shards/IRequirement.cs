using System;

namespace Kafe;

public interface IRequirement : IKafeTypeMetadata
{
    public static readonly string TypeCategory = "requirement";
}
