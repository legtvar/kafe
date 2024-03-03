using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Project;

[ApiVersion("1")]
[Route("project-review")]
[Authorize]
public class ProjectReviewEndpoint : EndpointBaseAsync
    .WithRequest<ProjectReviewCreationDto>
    .WithActionResult
{
    private readonly ProjectService projectService;
    private readonly IEmailService emailService;
    private readonly AccountService accountService;
    private readonly UserProvider userProvider;
    private readonly IAuthorizationService authorizationService;

    public ProjectReviewEndpoint(
        ProjectService projectService,
        IEmailService emailService,
        AccountService accountService,
        UserProvider userProvider,
        IAuthorizationService authorizationService)
    {
        this.projectService = projectService;
        this.emailService = emailService;
        this.accountService = accountService;
        this.userProvider = userProvider;
        this.authorizationService = authorizationService;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    public override async Task<ActionResult> HandleAsync(
        ProjectReviewCreationDto dto,
        CancellationToken cancellationToken = default)
    {
        var project = await projectService.Load(dto.ProjectId, cancellationToken);
        if (project is null)
        {
            return NotFound();
        }

        var auth = await authorizationService.AuthorizeAsync(User, project.ProjectGroupId, EndpointPolicy.Review);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var result = await projectService.AddReview(
            projectId: dto.ProjectId,
            kind: dto.Kind,
            reviewerRole: dto.ReviewerRole,
            comment: dto.Comment,
            token: cancellationToken);
        if (result.HasErrors)
        {
            return result.ToActionResult();
        }

        var owners = await accountService.List(
            filter: new(
                Permissions: ImmutableDictionary.CreateRange(new[]
                    {
                        new KeyValuePair<string, Permission>(dto.ProjectId.Value, Permission.Write)
                    })
            ),
            token: cancellationToken);

        if (dto.Comment is not null)
        {
            foreach (var owner in owners)
            {
                if (dto.Comment[owner.PreferredCulture] is not null)
                {
                    await emailService.SendEmail(
                        owner.EmailAddress,
                        Const.ProjectReviewEmailSubject[owner.PreferredCulture]!,
                        dto.Comment[owner.PreferredCulture]!,
                        userProvider.Account?.EmailAddress,
                        cancellationToken);
                }
            }
        }

        return Ok();
    }
}
