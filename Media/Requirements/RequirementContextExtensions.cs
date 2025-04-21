using System.Threading.Tasks;
using Kafe.Core.Diagnostics;
using Kafe.Core.Requirements;

namespace Kafe.Media.Requirements;

public static class RequirementContextExtensions
{
    public static async ValueTask<(IShard shard, MediaInfo mediaInfo)?> RequireMediaInfo(
        this IRequirementContext<IRequirement> context
    )
    {
        var pair = await context.RequireShardMetadata<MediaInfo>();
        if (pair.HasValue && pair.Value.metadata.IsCorrupted == true)
        {
            context.Report(new CorruptedShardDiagnostic(
                ShardName: pair.Value.shard.Name,
                ShardId: pair.Value.shard.Id,
                Variant: null
            ));
            return null;
        }

        return pair;
    }

    public static MediaInfo? RequireMediaInfo(
        this IShardRequirementContext<IRequirement> context
    )
    {
        var metadata = context.RequireShardMetadata<MediaInfo>();
        if (metadata is not null && metadata.IsCorrupted)
        {
            context.Report(new CorruptedShardDiagnostic(
                ShardName: context.Shard.Name,
                ShardId: context.Shard.Id,
                Variant: null
            ));
            return null;
        }

        return metadata;
    }
}
