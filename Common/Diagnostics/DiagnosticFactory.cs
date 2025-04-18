using System;

namespace Kafe;

public class DiagnosticFactory
{
    private readonly KafeTypeRegistry typeRegistry;
    private readonly DiagnosticDescriptorRegistry descriptorRegistry;
    private readonly KafeObjectFactory objectFactory;

    public DiagnosticFactory(
        KafeTypeRegistry typeRegistry,
        DiagnosticDescriptorRegistry descriptorRegistry,
        KafeObjectFactory objectFactory
    )
    {
        this.typeRegistry = typeRegistry;
        this.descriptorRegistry = descriptorRegistry;
        this.objectFactory = objectFactory;
    }

    public Diagnostic FromPayload(object payload, DiagnosticSeverity? severityOverride = null)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var payloadType = payload.GetType();
        if (!typeRegistry.DotnetTypeMap.TryGetValue(payloadType, out var kafeType))
        {
            throw new ArgumentException(
                $"No diagnostic type has a payload with .NET type '{payloadType}'.",
                nameof(payload)
            );
        }

        if (!descriptorRegistry.Metadata.TryGetValue(kafeType, out var descriptor))
        {
            throw new ArgumentException(
                $".NET type '{payloadType}' is registered as KAFE type '{kafeType}'. "
                    + $"However, '{kafeType}' is not a registered diagnostic descriptor."
            );
        }

        var payloadObject = objectFactory.Wrap(payload);
        return new Diagnostic(
            descriptor: descriptor,
            payload: payloadObject,
            severity: severityOverride,
            skipFrames: 2
        );
    }
}
