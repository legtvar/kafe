using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Kafe.Api.Endpoints.System;

[ApiVersion("1")]
[Route("system/video-conversion-retry")]
[Authorize(EndpointPolicy.Write)]
public class VideoConversionRetryEndpoint(
    VideoConversionService conversionService
) : EndpointBaseAsync
    .WithRequest<VideoConversionRetryDto>
    .WithActionResult<VideoConversionStatsDto>
{
    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.System])]
    public override async Task<ActionResult<VideoConversionStatsDto>> HandleAsync(
        VideoConversionRetryDto request,
        CancellationToken ct = default
    )
    {
        if (request.ShouldRetryConversion)
        {
            var result = await conversionService.RetryConversions(request.Ids, ct);
            if (result.HasErrors)
            {
                return this.KafeErrResult(result);
            }
        }

        if (request.ShouldRetryOriginalAnalysis)
        {
            var result = await conversionService.RetryOriginalAnalysis(request.Ids, ct);
            if (result.HasErrors)
            {
                return this.KafeErrResult(result);
            }
        }

        return Ok();
    }
}
