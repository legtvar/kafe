using System;
using System.Collections.Immutable;
using Serilog.Parsing;
using SmartFormat;

namespace Kafe;

public readonly record struct DiagnosticDescriptor : IInvalidable<DiagnosticDescriptor>
{
    public static readonly DiagnosticDescriptor Invalid = new DiagnosticDescriptor();

    static DiagnosticDescriptor IInvalidable<DiagnosticDescriptor>.Invalid => Invalid;

    public static readonly LocalizedString FallbackMessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "A diagnostic of type '{0}' has been reported."),
        (Const.CzechCulture, "Zpráva typu '{0}' byla nahlášena.")
    );

    public static readonly LocalizedString FallbackTitle = LocalizedString.Create(
        (Const.InvariantCulture, "A diagnostic of type '{0}'"),
        (Const.CzechCulture, "Hlášení typu '{0}'")
    );

    public DiagnosticDescriptor()
    {
    }

    public bool IsValid => Moniker != Const.InvalidId;

    /// <summary>
    /// A short name/ID that is unique within the mod.
    /// </summary>
    public string Moniker { get; init; } = Const.InvalidId;

    public Type PayloadType { get; init; } = typeof(void);

    public LocalizedString Title { get; init; } = LocalizedString.CreateInvariant(Const.InvalidName);

    public LocalizedString? Description { get; init; } = null;

    public LocalizedString MessageFormat { get; init; } = FallbackMessageFormat;

    public DiagnosticSeverity Severity { get; init; } = DiagnosticSeverity.Error;

    public static DiagnosticDescriptor FromPayloadType(Type payloadType)
    {
        if (!payloadType.IsAssignableTo(typeof(IDiagnosticPayload)))
        {
            throw new ArgumentException(
                $"A diagnostic payload must implement the {nameof(IDiagnosticPayload)} interface."
            );
        }

        var genericMethod = typeof(DiagnosticDescriptor).GetMethod(
            nameof(FromPayloadType),
            genericParameterCount: 1,
            []
        );
        if (genericMethod is null)
        {
            throw new InvalidOperationException(
                $"Failed to find the generic version of {nameof(FromPayloadType)}. This is likely a bug."
            );
        }

        return (DiagnosticDescriptor)genericMethod.MakeGenericMethod(payloadType).Invoke(null, [])!;
    }

    public static DiagnosticDescriptor FromPayloadType<T>()
        where T : IDiagnosticPayload
    {
        var moniker = T.Moniker;
        if (string.IsNullOrEmpty(moniker))
        {
            moniker = typeof(T).Name;
            moniker = Naming.WithoutSuffix(moniker, "Payload");
            moniker = Naming.WithoutSuffix(moniker, "Diagnostic");
            moniker = Naming.ToDashCase(moniker);
        }

        return new DiagnosticDescriptor
        {
            Moniker = moniker,
            PayloadType = typeof(T),
            Title = T.Title ?? LocalizedString.Format(FallbackTitle, moniker),
            Description = T.Description,
            MessageFormat = T.MessageFormat ?? LocalizedString.Format(FallbackMessageFormat, moniker),
            Severity = T.Severity
        };
    }

    public LocalizedString GetMessage(IDiagnosticPayload payload)
    {
        if (payload.GetType() != PayloadType)
        {
            throw new ArgumentException($"Payload must be of type '{PayloadType}'.", nameof(payload));
        }

        var messageBuilder = ImmutableDictionary.CreateBuilder<string, string>();
        foreach (var language in MessageFormat.Keys)
        {
            var localizedMessage = Smart.Format(MessageFormat[language], payload);
            messageBuilder[language] = localizedMessage;
        }

        return messageBuilder.ToImmutable();
    }
}
