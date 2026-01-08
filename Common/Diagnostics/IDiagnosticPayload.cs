using System;

namespace Kafe;

/// <summary>
/// The type and arguments of an <see cref="IDiagnostic"/>.
/// </summary>
public interface IDiagnosticPayload : IKafeTypeMetadata
{
    public static readonly string TypeCategory = "diagnostic";

    /// <summary>
    /// A long-form description of the diagnostic (without formatting placeholders).
    /// </summary>
    public static virtual LocalizedString? Description { get; }

    /// <summary>
    /// Message that will be composed for a specific instance of this diagnostic payload (with formatting placeholders).
    /// </summary>
    public static virtual LocalizedString? MessageFormat { get; }

    /// <summary>
    /// The default severity of this diagnostic type. Can be override during registration in a mod.
    /// </summary>
    public static virtual DiagnosticSeverity Severity { get; }
}
