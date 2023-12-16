using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Api.Services;
using Kafe.Data.Services;
using System.Linq;
using System.Collections.Immutable;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Reflection;
using JasperFx.Core;
using System;
using System.Collections.Generic;

namespace Kafe.Api.Endpoints.ProjectGroup;

[ApiVersion("1")]
[Route("system")]
[Authorize(EndpointPolicy.Read)]
public class SystemDetailEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<SystemDetailDto>
{
    private readonly IHostEnvironment hostEnvironment;
    private readonly IServer server;

    public SystemDetailEndpoint(
        IHostEnvironment hostEnvironment,
        IServer server)
    {
        this.hostEnvironment = hostEnvironment;
        this.server = server;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.System })]
    public override Task<ActionResult<SystemDetailDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var dto = new SystemDetailDto(
            Name: hostEnvironment.ApplicationName,
            BaseUrls: (server.Features.Get<IServerAddressesFeature>()?.Addresses ?? Enumerable.Empty<string>())
                .ToImmutableArray(),
            Version: typeof(Startup).Assembly.GetName().Version?.ToString() ?? "unknown",
            Commit: ThisAssembly.Git.Commit,
            CommitDate: DateTimeOffset.Parse(ThisAssembly.Git.CommitDate),
            RunningSince: System.Diagnostics.Process.GetCurrentProcess().StartTime
        );
        return Task.FromResult<ActionResult<SystemDetailDto>>(Ok(dto));
    }
}
