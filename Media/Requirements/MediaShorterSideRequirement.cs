using System;
using System.Threading.Tasks;
using Kafe.Core.Requirements;
using Kafe.Media.Diagnostics;

namespace Kafe.Media.Requirements;

public record MediaShorterSideRequirement(
    int Min
) : IRequirement
{
    public static string Moniker { get; } = "shorter-side";
}

public class MediaShorterSideRequirementHandler : ShardRequirementHandlerBase<MediaShorterSideRequirement>
{
    public override ValueTask Handle(IShardRequirementContext<MediaShorterSideRequirement> context)
    {
        if (context.Shard.Payload.Value is ImageInfo imageInfo)
        {
            if (Math.Min(imageInfo.Width, imageInfo.Height) < context.Requirement.Min)
            {
                context.Report(new MediaShorterSideTooShortDiagnostic(
                    context.Shard.Name,
                    context.Shard.Id,
                    null,
                    context.Requirement.Min
                ));
                return ValueTask.CompletedTask;
            }
        }

        var mediaInfo = context.RequireMediaInfo();
        if (mediaInfo is null)
        {
            return ValueTask.CompletedTask;
        }

        foreach(var videoStream in mediaInfo.VideoStreams)
        {
            if (Math.Min(videoStream.Width, videoStream.Height) < context.Requirement.Min)
            {
                context.Report(new MediaShorterSideTooShortDiagnostic(
                    context.Shard.Name,
                    context.Shard.Id,
                    null,
                    context.Requirement.Min
                ));
                return ValueTask.CompletedTask;
            }
        }

        return ValueTask.CompletedTask;
    }
}
