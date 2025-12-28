using System;
using System.Threading.Tasks;
using Kafe.Core.Diagnostics;

namespace Kafe.Core;

public record AuthorReferenceProperty(
    Hrib? AuthorId,
    string? Name,
    string[] Roles
) : IPropertyType
{
    public static string Moniker { get; } = "author-ref";
}

public sealed record AuthorReferenceNameOrIdRequirement : IRequirement
{
    public static string Moniker { get; } = "author-ref-name-or-id";

    public static KafeTypeAccessibility Accessibility { get; } = KafeTypeAccessibility.Internal;
}

public sealed class AuthorReferenceNameOrIdRequirementHandler
    : RequirementHandlerBase<AuthorReferenceNameOrIdRequirement>
{
    public override ValueTask Handle(IRequirementContext<AuthorReferenceNameOrIdRequirement> context)
    {
        if (context.Target is not AuthorReferenceProperty authorRef)
        {
            throw new InvalidOperationException($"{nameof(AuthorReferenceNameOrIdRequirement)} is not valid "
                + $"on objects of type '{context.Target.GetType()}'.");
        }

        if (authorRef.Name is null && authorRef.AuthorId is null)
        {
            context.Report(new MissingNameOrIdDiagnostic(context.Target.Type));
        }

        return ValueTask.CompletedTask;
    }
}
