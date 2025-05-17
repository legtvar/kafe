using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Kafe.Core.Requirements;
using Kafe.Media.Diagnostics;

namespace Kafe.Media.Requirements;

public record MediaStreamCountRequirement(
    int? Min,
    int? Max,
    MediaStreamKind Kind
) : IRequirement
{
    public static string Moniker { get; } = "stream-count";
}

public class MediaStreamCountRequirementHandler : ShardRequirementHandlerBase<MediaStreamCountRequirement>
{
    public override ValueTask Handle(IShardRequirementContext<MediaStreamCountRequirement> context)
    {
        var mediaInfo = context.RequireMediaInfo();
        if (mediaInfo is null)
        {
            return ValueTask.CompletedTask;
        }

        throw new NotImplementedException();
    }
}
