using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Kafe.Core.Requirements;
using Kafe.Media.Diagnostics;

namespace Kafe.Media.Requirements;

public record VideoSubtitlesRequirement(
    string Language
) : IRequirement
{
    public static string Moniker => "video-subtitles";
}

public class VideoSubtitlesRequirementHandler : ShardRequirementHandlerBase<VideoFramerateRequirement>
{
    public override ValueTask Handle(IShardRequirementContext<VideoFramerateRequirement> context)
    {
        var mediaInfo = context.RequireMediaInfo();
        if (mediaInfo is null)
        {
            return ValueTask.CompletedTask;
        }

        throw new NotImplementedException();
    }
}
