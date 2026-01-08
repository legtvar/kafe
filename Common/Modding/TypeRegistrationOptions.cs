using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kafe;

public record TypeRegistrationOptions
{
    public KafeTypeAccessibility Accessibility { get; set; } = KafeTypeAccessibility.Public;

    public JsonConverter? Converter { get; set; } = null;

    public string? Moniker { get; set; } = null;

    public LocalizedString? Title { get; set; } = null;
}

public record ScalarRegistrationOptions : TypeRegistrationOptions
{
    public List<IRequirement> DefaultRequirements { get; set; } = [];
}

public record RequirementRegistrationOptions : TypeRegistrationOptions
{
    public List<Type> HandlerTypes { get; set; } = [];
}

public record DiagnosticPayloadRegistrationOptions : TypeRegistrationOptions
{
    public DiagnosticDescriptor? OverrideDescriptor { get; set; }
}

public record ShardPayloadRegistrationOptions : TypeRegistrationOptions
{
    public List<Type> AnalyzerTypes { get; set; } = [];
}
