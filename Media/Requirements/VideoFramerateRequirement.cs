using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Kafe.Core.Requirements;
using Kafe.Media.Diagnostics;

namespace Kafe.Media.Requirements;

public record VideoFramerateRequirement(
    ImmutableArray<double> Include,
    ImmutableArray<double> Exclude,
    int? StreamIndex = null
) : IRequirement
{
    public static string Moniker => "video-framerate";
}

public class VideoFramerateRequirementHandler : ShardRequirementHandlerBase<VideoFramerateRequirement>
{
    public override ValueTask Handle(IShardRequirementContext<VideoFramerateRequirement> context)
    {
        var mediaInfo = context.RequireMediaInfo();
        if (mediaInfo is null)
        {
            return ValueTask.CompletedTask;
        }

        var allowedFramerates = context.Requirement.Include
            .Except(context.Requirement.Exclude)
            .ToImmutableArray()
            .Sort();

        if (context.Requirement.StreamIndex.HasValue)
        {
            if (mediaInfo.VideoStreams.Length <= context.Requirement.StreamIndex)
            {
                context.Report(new MissingVideoStreamDiagnostic(
                    ShardName: context.Shard.Name,
                    ShardId: context.Shard.Id,
                    Variant: null,
                    StreamIndex: context.Requirement.StreamIndex.Value
                ));
                return ValueTask.CompletedTask;
            }
            CheckStream(
                context,
                mediaInfo.VideoStreams[context.Requirement.StreamIndex.Value],
                context.Requirement.StreamIndex.Value,
                allowedFramerates
            );
            return ValueTask.CompletedTask;
        }

        for (int streamIndex = 0; streamIndex < mediaInfo.VideoStreams.Length; ++streamIndex)
        {
            CheckStream(
                context,
                mediaInfo.VideoStreams[streamIndex],
                streamIndex,
                allowedFramerates
            );
        }
        return ValueTask.CompletedTask;
    }

    private static void CheckStream(
        IShardRequirementContext<VideoFramerateRequirement> context,
        VideoStreamInfo videoStream,
        int streamIndex,
        ImmutableArray<double> allowedFramerates
    )
    {
        if (!allowedFramerates.Contains(videoStream.Framerate))
        {
            context.Report(new VideoFramerateNotAllowedDiagnostic(
                context.Shard.Id,
                context.Shard.Name,
                videoStream.Framerate,
                allowedFramerates,
                streamIndex
            ));
        }
    }
}
