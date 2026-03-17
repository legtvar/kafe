using System;
using System.Text.Json.Serialization;

namespace Kafe;

/// <summary>
/// A storage and transport wrapper for <see cref="IKafeTypeMetadata"/>.
/// </summary>
public readonly record struct KafeObject(
    KafeType Type,
    object Value
) : IInvalidable
{
    public static readonly KafeObject Invalid = new(
        Type: KafeType.Invalid,
        Value: null!
    );

    /// <summary>
    /// The <see cref="Type.FullName"/> of the .NET type corresponding to <see cref="Type"/>.
    /// </summary>
    /// <remarks>
    /// Used only when KAFE objects are stored in the database, so that they can be upgraded,
    /// similarly to Marten's upcasters.
    /// </remarks>
    public string? DotnetType => Type.DotnetType?.FullName;

    [JsonIgnore]
    public bool IsValid => Type != KafeType.Invalid && Value is not null;
}
