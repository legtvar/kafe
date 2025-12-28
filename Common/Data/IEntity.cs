using System;

namespace Kafe;

public interface IEntity
{
    /// <summary>
    /// A short, dash-case name of the entity. If set, becomes <see cref="KafeType.Secondary"/>.
    /// </summary>
    public static virtual string? Moniker { get; }

    /// <summary>
    /// A human-readable name of the entity type.
    /// </summary>
    public static virtual LocalizedString? Title { get; }

    // NB: For the time being, Id is string despite always being a Hrib because of Marten
    Hrib Id { get; }
}
