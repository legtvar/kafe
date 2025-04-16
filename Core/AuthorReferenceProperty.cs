using System;
using System.Threading.Tasks;
using Kafe.Core.Diagnostics;

namespace Kafe.Core;

public record AuthorReferenceProperty(
    Hrib? AuthorId,
    string? Name,
    string[] Roles
);

public sealed record AuthorReferenceNameOrIdRequirement : IRequirement;

public sealed class AuthorReferenceNameOrIdRequirementHandler
    : RequirementHandlerBase<AuthorReferenceNameOrIdRequirement>
{
    public override ValueTask Handle(RequirementContext context)
    {
        if (context.Object.Value is not AuthorReferenceProperty authorRef)
        {
            throw new InvalidOperationException($"{nameof(AuthorReferenceNameOrIdRequirement)} is not valid "
                + $"on objects of type '{context.Object.Type}'.");
        }

        if (authorRef.Name is null && authorRef.AuthorId is null)
        {
            context.Report(new MissingNameOrIdDiagnostic(context.Object.Type));
        }

        return ValueTask.CompletedTask;
    }
}
