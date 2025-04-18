namespace Kafe;

public interface IDiagnosticPayload
{
    /// <summary>
    /// Short identifier used when registering this diagnostic payload type in <see cref="ModContext"/>
    /// and creating a <see cref="DiagnosticDescriptor"/> based on it.
    /// </summary>
    public static string? Name { get; }
    public static virtual LocalizedString? Title { get; }
    public static virtual LocalizedString? Description { get; }
    public static virtual LocalizedString? MessageFormat { get; }
    public static virtual string? HelpLinkUri { get; }
    public static virtual DiagnosticSeverity? DefaultSeverity { get; }
}
