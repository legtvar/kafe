using System;
using System.Threading.Tasks;
using Kafe.Media.Diagnostics;

namespace Kafe.Media.Requirements;

public record MediaDurationRequirement(
    TimeSpan? Min,
    TimeSpan? Max
) : IRequirement
{
    public static string Moniker { get; } = "media-duration";
}

public class MediaDurationRequirementHandler : RequirementHandlerBase<MediaDurationRequirement>
{
    public static readonly TimeSpan AcceptableDurationError = TimeSpan.FromSeconds(1);

    public override async ValueTask Handle(IRequirementContext<MediaDurationRequirement> context)
    {
        if (context.Requirement.Min is null && context.Requirement.Max is null)
        {
            // TODO: warn about the uselessness of the user's doing.
            return;
        }

        var (shard, mediaInfo) = await context.RequireMediaInfo() ?? default;
        if (shard is null || mediaInfo is null)
        {
            return;
        }

        if (context.Requirement.Min is not null
            && mediaInfo.Duration + AcceptableDurationError < context.Requirement.Min.Value)
        {
            context.Report(new MediaTooShortDiagnostic(
                ShardName: shard.Name,
                ShardId: shard.Id,
                Variant: Const.OriginalShardVariant,
                MinDuration: context.Requirement.Min.Value
            ));
        }

        if (context.Requirement.Max is not null
            && mediaInfo.Duration - AcceptableDurationError > context.Requirement.Max.Value)
        {
            context.Report(new MediaTooLongDiagnostic(
                ShardName: shard.Name,
                ShardId: shard.Id,
                Variant: Const.OriginalShardVariant,
                MaxDuration: context.Requirement.Max.Value
            ));
        }
    }
}
