using System;
using System.Threading.Tasks;
using Kafe.Core;
using Kafe.Core.Diagnostics;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Media.Diagnostics;

namespace Kafe.Media.Requirements;

public record MediaDurationRequirement(
    TimeSpan? MinDuration,
    TimeSpan? MaxDuration
) : IRequirement
{
    public static string Name { get; } = "media-duration";
}

public class MediaDurationRequirementHandler : RequirementHandlerBase<MediaDurationRequirement>
{
    private readonly IKafeQuerySession db;

    public static readonly TimeSpan AcceptableDurationError = TimeSpan.FromSeconds(1);

    public MediaDurationRequirementHandler(IKafeQuerySession db)
    {
        this.db = db;
    }

    public override async ValueTask Handle(IRequirementContext<MediaDurationRequirement> context)
    {
        if (context.Requirement.MinDuration is null && context.Requirement.MaxDuration is null)
        {
            // TODO: warn about the uselessness of the user's doing.
            return;
        }

        if (context.Object.Value is not ShardReferenceProperty shardRef)
        {
            context.Report(new IncompatibleRequirementDiagnostic(
                context.RequirementType,
                context.Object.Type
            ));
            return;
        }

        var shard = await db.LoadAsync<ShardInfo>(shardRef.ShardId, context.CancellationToken);
        if (shard.Diagnostic is not null)
        {
            context.Report(shard.Diagnostic);
            return;
        }

        // TODO: replace with real shard names, once shard have names
        var shardName = LocalizedString.CreateInvariant(shard.Value.Id);

        if (!shard.Value.Variants.TryGetValue(Const.OriginalShardVariant, out var variantObject))
        {
            context.Report(new MissingShardVariantDiagnostic(
                ShardName: shardName,
                ShardId: shardRef.ShardId,
                Variant: Const.OriginalShardVariant
            ));
            return;
        }

        if (variantObject.Value is not MediaInfo mediaInfo)
        {
            context.Report(new IncompatibleRequirementDiagnostic(
                context.RequirementType,
                variantObject.Type
            ));
            return;
        }

        if (mediaInfo.IsCorrupted)
        {
            context.Report(new CorruptedShardVariantDiagnostic(
                ShardName: shardName,
                ShardId: shardRef.ShardId,
                Variant: Const.OriginalShardVariant
            ));
            return;
        }

        if (context.Requirement.MinDuration is not null
            && mediaInfo.Duration + AcceptableDurationError < context.Requirement.MinDuration.Value)
        {
            context.Report(new MediaTooShortDiagnostic(
                ShardName: shardName,
                ShardId: shardRef.ShardId,
                Variant: Const.OriginalShardVariant,
                MinDuration: context.Requirement.MinDuration.Value
            ));
        }

        if (context.Requirement.MaxDuration is not null
            && mediaInfo.Duration - AcceptableDurationError > context.Requirement.MaxDuration.Value)
        {
            context.Report(new MediaTooLongDiagnostic(
                ShardName: shardName,
                ShardId: shardRef.ShardId,
                Variant: Const.OriginalShardVariant,
                MaxDuration: context.Requirement.MaxDuration.Value
            ));
        }
    }
}
