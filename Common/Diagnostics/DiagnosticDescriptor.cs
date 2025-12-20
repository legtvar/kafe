
using System;

namespace Kafe;

public sealed record DiagnosticDescriptor : ISubtypeMetadata, IInvalidable<DiagnosticDescriptor>
{
    public static readonly DiagnosticDescriptor Invalid = new DiagnosticDescriptor();

    static DiagnosticDescriptor IInvalidable<DiagnosticDescriptor>.Invalid => Invalid;

    public static readonly LocalizedString FallbackMessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "A diagnostic of type '{0}' has been reported."),
        (Const.CzechCulture, "Zpráva typu '{0}' byla nahlášena.")
    );

    public bool IsValid => Id != Const.InvalidId;

    /// <summary>
    /// A short name/ID that is unique within the mod.
    /// </summary>
    public string Id { get; init; } = Const.InvalidId;

    public KafeType KafeType { get; init; }

    public Type DotnetType { get; init; } = typeof(void);

    public LocalizedString Title { get; init; } = LocalizedString.CreateInvariant(Const.InvalidName);

    public LocalizedString? Description { get; init; }

    public string? HelpLinkUri { get; init; }

    public LocalizedString MessageFormat { get; init; } = LocalizedString.CreateInvariant(Const.InvalidName);

    public DiagnosticSeverity DefaultSeverity { get; init; } = DiagnosticSeverity.Error;
}
