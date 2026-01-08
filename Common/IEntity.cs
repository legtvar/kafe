using System;

namespace Kafe;

public interface IEntity : IKafeTypeMetadata
{
    public static readonly string TypeCategory = "entity";

    // NB: For the time being, Id is string despite always being a Hrib because of Marten
    Hrib Id { get; }
}
