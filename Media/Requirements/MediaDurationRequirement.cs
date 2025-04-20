using System;
using System.Threading.Tasks;
using Kafe.Core.Diagnostics;
using Kafe.Data.Requirements;
using Kafe.Media.Diagnostics;

namespace Kafe.Media.Requirements;

public record MediaDurationRequirement(
    TimeSpan? MinDuration,
    TimeSpan? MaxDuration
) : IRequirement
{
    public static string Moniker { get; } = "media-duration";
}

public class MediaDurationRequirementHandler : RequirementHandlerBase<MediaDurationRequirement>
{
    public static readonly TimeSpan AcceptableDurationError = TimeSpan.FromSeconds(1);

    public override async ValueTask Handle(IRequirementContext<MediaDurationRequirement> context)
    {
        if (context.Requirement.MinDuration is null && context.Requirement.MaxDuration is null)
        {
            // TODO: warn about the uselessness of the user's doing.
            return;
        }

        var (shard, mediaInfo) = await context.RequireShardVariant<MediaInfo>(Const.OriginalShardVariant) ?? default;
        if (shard is null || mediaInfo is null)
        {
            return;
        }

        // TODO: replace with real shard names, once shard have names
        var shardName = LocalizedString.CreateInvariant(shard.Id);

        if (mediaInfo.IsCorrupted)
        {
            context.Report(new CorruptedShardDiagnostic(
                ShardName: shardName,
                ShardId: shard.Id,
                Variant: Const.OriginalShardVariant
            ));
            return;
        }

        if (context.Requirement.MinDuration is not null
            && mediaInfo.Duration + AcceptableDurationError < context.Requirement.MinDuration.Value)
        {
            context.Report(new MediaTooShortDiagnostic(
                ShardName: shardName,
                ShardId: shard.Id,
                Variant: Const.OriginalShardVariant,
                MinDuration: context.Requirement.MinDuration.Value
            ));
        }

        if (context.Requirement.MaxDuration is not null
            && mediaInfo.Duration - AcceptableDurationError > context.Requirement.MaxDuration.Value)
        {
            context.Report(new MediaTooLongDiagnostic(
                ShardName: shardName,
                ShardId: shard.Id,
                Variant: Const.OriginalShardVariant,
                MaxDuration: context.Requirement.MaxDuration.Value
            ));
        }
    }
}
