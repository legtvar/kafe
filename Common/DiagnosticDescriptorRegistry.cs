using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Kafe;

public class DiagnosticDescriptorRegistry : IFreezable
{
    private readonly ConcurrentDictionary<KafeType, DiagnosticDescriptor> descriptors = new();

    public bool IsFrozen { get; private set; }

    public IReadOnlyDictionary<KafeType, DiagnosticDescriptor> Descriptors { get; }

    public DiagnosticDescriptorRegistry()
    {
        Descriptors = descriptors.AsReadOnly();
    }

    public void Freeze()
    {
        IsFrozen = true;
    }

    public DiagnosticDescriptorRegistry Register(DiagnosticDescriptor descriptor)
    {
        AssertUnfrozen();
        if (!descriptors.TryAdd(descriptor.KafeType, descriptor))
        {
            throw new ArgumentException(
                $"Diagnostic descriptor '{descriptor.Id}' has been already registered.",
                nameof(descriptor)
            );
        }
        return this;
    }

    private void AssertUnfrozen()
    {
        if (IsFrozen)
        {
            throw new InvalidOperationException("This diagnostic descriptor registry is frozen "
                + "and can no longer be modified.");
        }
    }
}
