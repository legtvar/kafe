using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System;
using System.Diagnostics;

namespace Kafe.Api.Endpoints.System;

[ApiVersion("1")]
[Route("system")]
[Authorize(EndpointPolicy.Read)]
public class SystemDetailEndpoint(
    IHostEnvironment hostEnvironment,
    IServer server
) : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<SystemDetailDto>
{
    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.System])]
    public override Task<ActionResult<SystemDetailDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var dto = new SystemDetailDto(
            Name: hostEnvironment.ApplicationName,
            BaseUrls: [..(server.Features.Get<IServerAddressesFeature>()?.Addresses ?? Enumerable.Empty<string>())],
            Version: ThisAssembly.Git.Tag,
            Commit: ThisAssembly.Git.Commit,
            CommitDate: DateTimeOffset.Parse(ThisAssembly.Git.CommitDate),
            RunningSince: Process.GetCurrentProcess().StartTime
        );
        return Task.FromResult<ActionResult<SystemDetailDto>>(Ok(dto));
    }
}
