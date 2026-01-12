using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Kafe;

public record KafeTypeRegistrationOptions
{
    public KafeTypeAccessibility Accessibility { get; set; } = KafeTypeAccessibility.Public;

    public JsonConverter? Converter { get; set; } = null;

    public string? Moniker { get; set; } = null;

    public LocalizedString? Title { get; set; } = null;

    public ISet<Type> Aliases { get; set; } = ImmutableHashSet.CreateBuilder<Type>();
}

public record ScalarRegistrationOptions : KafeTypeRegistrationOptions
{
    public IList<IRequirement> DefaultRequirements { get; set; } = ImmutableArray.CreateBuilder<IRequirement>();
}

public record RequirementRegistrationOptions : KafeTypeRegistrationOptions
{
    public IList<Type> HandlerTypes { get; set; } = ImmutableArray.CreateBuilder<Type>();
}

public record DiagnosticPayloadRegistrationOptions : KafeTypeRegistrationOptions
{
    public DiagnosticDescriptor? OverrideDescriptor { get; set; }
}

public record ShardPayloadRegistrationOptions : KafeTypeRegistrationOptions
{
    public IList<Type> AnalyzerTypes { get; set; } = ImmutableArray.CreateBuilder<Type>();
}
