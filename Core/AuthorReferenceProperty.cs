using System.Threading.Tasks;

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
    public const string MissingAuthorNameOrId = "CORE0001";

    public const string NotAuthorReference = "CORE0002";

    public override ValueTask Handle(RequirementContext context)
    {
        if (context.Object.Value is not AuthorReferenceProperty authorRef)
        {
            context.ReportError(NotAuthorReference, LocalizedString.CreateInvariant(
                "This requirement is not valid on objects of types other than author reference."));
            return ValueTask.CompletedTask;
        }

        if (authorRef.Name is null && authorRef.AuthorId is null)
        {
            context.ReportError(MissingAuthorNameOrId, LocalizedString.CreateInvariant(
                $"An author reference must have the author's ID, name, or both set."
            ));
        }

        return ValueTask.CompletedTask;
    }
}
