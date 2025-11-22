using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Kafe.Data.Services;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Kafe.Api.Endpoints.System;

[ApiVersion("1")]
[Route("system/video-conversion-stats")]
[Authorize(EndpointPolicy.Read)]
public class VideoConversionStatsEndpoint(
    VideoConversionService conversionService
) : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<VideoConversionStatsDto>
{
    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.System])]
    public override async Task<ActionResult<VideoConversionStatsDto>> HandleAsync(CancellationToken ct = default)
    {
        var dto = new VideoConversionStatsDto(
            TotalVideoShardCount: await conversionService.QueryVideoShards(
                new VideoConversionService.VideoShardFilter(
                    HasOriginalVariant: null,
                    IsCorrupted: null,
                    HasCompletedOrFailedConversion: null)
            ).CountAsync(ct),
            CorruptedVideoShardCount: await conversionService.QueryVideoShards(
                new VideoConversionService.VideoShardFilter(
                    HasOriginalVariant: null,
                    IsCorrupted: true,
                    HasCompletedOrFailedConversion: null)
            ).CountAsync(ct),
            PendingVideoConversionCount: await conversionService.QueryVideoShards(
                new VideoConversionService.VideoShardFilter(
                    HasOriginalVariant: true,
                    IsCorrupted: false,
                    HasCompletedOrFailedConversion: false)
            ).CountAsync(ct)
        );
        return Ok(dto);
    }
}
