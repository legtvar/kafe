using System;

namespace Kafe;

public class DiagnosticFactory
{
    public DiagnosticFactory(
        KafeTypeRegistry typeRegistry,
        DiagnosticDescriptorRegistry descriptorRegistry,
        KafeObjectFactory objectFactory
    )
    {
        TypeRegistry = typeRegistry;
        DescriptorRegistry = descriptorRegistry;
        ObjectFactory = objectFactory;
    }

    public KafeTypeRegistry TypeRegistry { get; }
    public DiagnosticDescriptorRegistry DescriptorRegistry { get; }
    public KafeObjectFactory ObjectFactory { get; }

    public Diagnostic FromPayload(object payload, DiagnosticSeverity? severityOverride = null)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var payloadType = payload.GetType();
        if (!TypeRegistry.DotnetTypeMap.TryGetValue(payloadType, out var kafeType))
        {
            throw new ArgumentException(
                $"No diagnostic type has a payload with .NET type '{payloadType}'.",
                nameof(payload)
            );
        }

        if (!DescriptorRegistry.Metadata.TryGetValue(kafeType, out var descriptor))
        {
            throw new ArgumentException(
                $".NET type '{payloadType}' is registered as KAFE type '{kafeType}'. "
                    + $"However, '{kafeType}' is not a registered diagnostic descriptor."
            );
        }

        var payloadObject = ObjectFactory.Wrap(payload);
        return new Diagnostic(
            descriptor: descriptor,
            payload: payloadObject,
            severity: severityOverride,
            skipFrames: 2
        );
    }
}
