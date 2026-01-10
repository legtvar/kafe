using System;
using System.Threading.Tasks;
using Kafe.Core.Requirements;
using Kafe.Media.Diagnostics;

namespace Kafe.Media.Requirements;

public record MediaDurationRequirement(
    TimeSpan? Min,
    TimeSpan? Max
) : IRequirement
{
    public static string Moniker => "media-duration";
}

public class MediaDurationRequirementHandler : ShardRequirementHandlerBase<MediaDurationRequirement>
{
    public static readonly TimeSpan AcceptableDurationError = TimeSpan.FromSeconds(1);

    public override ValueTask Handle(IShardRequirementContext<MediaDurationRequirement> context)
    {
        if (context.Requirement.Min is null && context.Requirement.Max is null)
        {
            // TODO: warn about the uselessness of the user's doing.
            return ValueTask.CompletedTask;
        }

        var mediaInfo = context.RequireMediaInfo();
        if (mediaInfo is null)
        {
            return ValueTask.CompletedTask;
        }

        if (context.Requirement.Min is not null
            && mediaInfo.Duration + AcceptableDurationError < context.Requirement.Min.Value)
        {
            context.Report(new MediaTooShortDiagnostic(
                ShardName: context.Shard.Name,
                ShardId: context.Shard.Id,
                Variant: Const.OriginalShardVariant,
                MinDuration: context.Requirement.Min.Value
            ));
        }

        if (context.Requirement.Max is not null
            && mediaInfo.Duration - AcceptableDurationError > context.Requirement.Max.Value)
        {
            context.Report(new MediaTooLongDiagnostic(
                ShardName: context.Shard.Name,
                ShardId: context.Shard.Id,
                Variant: Const.OriginalShardVariant,
                MaxDuration: context.Requirement.Max.Value
            ));
        }

        return ValueTask.CompletedTask;
    }
}
