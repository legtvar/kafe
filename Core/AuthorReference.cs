using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Kafe.Core.Diagnostics;

namespace Kafe.Core;

public record AuthorReference(
    Hrib? AuthorId,
    string? Name,
    // TODO: Replace/add Tags once implemented.
    ImmutableArray<LocalizedString> Roles
) : IScalar
{
    public static string Moniker => "author-ref";
}

public sealed record AuthorReferenceNameOrIdRequirement : IRequirement
{
    public static string Moniker => "author-ref-name-or-id";

    public static KafeTypeAccessibility Accessibility { get; } = KafeTypeAccessibility.Internal;
}

public sealed class AuthorReferenceNameOrIdRequirementHandler
    : RequirementHandlerBase<AuthorReferenceNameOrIdRequirement>
{
    public override ValueTask Handle(IRequirementContext<AuthorReferenceNameOrIdRequirement> context)
    {
        if (context.Target is not AuthorReference authorRef)
        {
            throw new InvalidOperationException($"{nameof(AuthorReferenceNameOrIdRequirement)} is not valid "
                + $"on objects of type '{context.Target.GetType()}'.");
        }

        if (authorRef.Name is null && authorRef.AuthorId is null)
        {
            context.Report(new MissingAuthorNameOrIdDiagnostic());
        }

        return ValueTask.CompletedTask;
    }
}
