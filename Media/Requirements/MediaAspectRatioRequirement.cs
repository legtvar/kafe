using System;
using System.Threading.Tasks;
using Kafe.Core.Requirements;
using Kafe.Media.Diagnostics;

namespace Kafe.Media.Requirements;

public record MediaAspectRatioRequirement(
    string? Min,
    string? Max
) : IRequirement
{
    public static string Moniker { get; } = "aspect-ratio";
}

public class MediaAspectRatioRequirementHandler : ShardRequirementHandlerBase<MediaAspectRatioRequirement>
{
    public override ValueTask Handle(IShardRequirementContext<MediaAspectRatioRequirement> context)
    {
        throw new NotImplementedException();
    }
}
