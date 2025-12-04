using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Api.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Api.Services;
using Kafe.Data.Services;

namespace Kafe.Api.Endpoints.Project;

[ApiVersion("1")]
[Route("project/{id}")]
public class ProjectDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<ProjectDetailDto>
{
    private readonly ProjectService projectService;
    private readonly ProjectGroupService projectGroupService;
    private readonly EntityService entityService;
    private readonly ArtifactService artifactService;
    private readonly AuthorService authorService;
    private readonly UserProvider userProvider;
    private readonly IAuthorizationService authorizationService;

    public ProjectDetailEndpoint(
        ProjectService projectService,
        ProjectGroupService projectGroupService,
        EntityService entityService,
        ArtifactService artifactService,
        AuthorService authorService,
        UserProvider userProvider,
        IAuthorizationService authorizationService)
    {
        this.projectService = projectService;
        this.projectGroupService = projectGroupService;
        this.entityService = entityService;
        this.artifactService = artifactService;
        this.authorService = authorService;
        this.userProvider = userProvider;
        this.authorizationService = authorizationService;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    [ProducesResponseType(typeof(ProjectDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<ProjectDetailDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, id, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var project = await projectService.Load(id, cancellationToken);
        if (project is null)
        {
            return NotFound();
        }

        var group = await projectGroupService.Load(project.ProjectGroupId, cancellationToken);
        if (group is null)
        {
            return NotFound();
        }

        var userPerms = await entityService.GetPermission(project.Id, userProvider.AccountId, cancellationToken);

        var dto = TransferMaps.ToProjectDetailDto(project, userPerms) with
        {
            ProjectGroupName = group.Name,
            ValidationSettings = ProjectValidationSettings.Merge(
                group.ValidationSettings,
                ProjectValidationSettings.Default
            ),
            Blueprint = group.OrganizationId == "mate-fimuni"
            ? TransferMaps.TemporaryMateProjectBlueprintMockup
            : TransferMaps.TemporaryProjectBlueprintMockup
        };

        var artifactDetails = await artifactService.LoadDetailMany(
            project.Artifacts.Select(a => (Hrib)a.Id).Distinct(),
            cancellationToken);
        var authors = await authorService.LoadMany(
            project.Authors.Select(a => (Hrib)a.Id).Distinct(),
            cancellationToken);

        dto = dto with
        {
            ProjectGroupName = group?.Name ?? Const.UnknownProjectGroup,
            Artifacts = artifactDetails
                .Select(a => TransferMaps.ToProjectArtifactDto(a) with {
                    BlueprintSlot = project.Artifacts.FirstOrDefault(b => b.Id == a.Id)?.BlueprintSlot
                })
                .ToImmutableArray(),
            Cast = project.Authors.Where(a => a.Kind == ProjectAuthorKind.Cast)
                    .Select(a => new ProjectAuthorDto(
                        Id: a.Id,
                        Name: authors.SingleOrDefault(e => e?.Id == a.Id)?.Name ?? (string)Const.UnknownAuthor,
                        Roles: a.Roles))
                    .ToImmutableArray(),
            Crew = project.Authors.Where(a => a.Kind == ProjectAuthorKind.Crew)
                    .Select(a => new ProjectAuthorDto(
                        Id: a.Id,
                        Name: authors.SingleOrDefault(e => e?.Id == a.Id)?.Name ?? (string)Const.UnknownAuthor,
                        Roles: a.Roles))
                    .ToImmutableArray()
        };

        return Ok(dto);
    }
}
