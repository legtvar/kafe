using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Data.Services;
using Kafe.Api.Services;

namespace Kafe.Api.Endpoints.Author;

[ApiVersion("1")]
[Route("author/{id}")]
public class AuthorDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<AuthorDetailDto?>
{
    private readonly AuthorService authorService;
    private readonly EntityService entityService;
    private readonly UserProvider userProvider;
    private readonly IAuthorizationService authorizationService;

    public AuthorDetailEndpoint(
        AuthorService authorService,
        EntityService entityService,
        UserProvider userProvider,
        IAuthorizationService authorizationService)
    {
        this.authorService = authorService;
        this.entityService = entityService;
        this.userProvider = userProvider;
        this.authorizationService = authorizationService;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Author })]
    [ProducesResponseType(typeof(AuthorDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<AuthorDetailDto?>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, id, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }
        
        var data = await authorService.Load(id, cancellationToken);
        if (data is null)
        {
            return NotFound();
        }

        var userPerms = await entityService.GetPermission(id, userProvider.AccountId, cancellationToken);
        return Ok(TransferMaps.ToAuthorDetailDto(data, userPerms));
    }
}
