using System.Threading.Tasks;
using Kafe.Core;
using Kafe.Core.Diagnostics;
using Kafe.Data.Aggregates;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe.Data.Requirements;

public static class RequirementContextExtensions
{
    public static async Task<ShardInfo?> RequireShard(this IRequirementContext<IRequirement> context)
    {
        if (context.Object.Value is not ShardReferenceProperty shardRef)
        {
            context.Report(new IncompatibleRequirementDiagnostic(
                context.RequirementType,
                context.Object.Type
            ));
            return null;
        }

        var db = context.ServiceProvider.GetRequiredService<IKafeQuerySession>();
        var shard = await db.LoadAsync<ShardInfo>(shardRef.ShardId, context.CancellationToken);
        if (shard.Diagnostic is not null)
        {
            context.Report(shard.Diagnostic);
            return null;
        }

        return shard.Value;
    }

    public static async Task<(ShardInfo shard, KafeObject variant)?> RequireShardVariant(
        this IRequirementContext<IRequirement> context,
        string variant
    )
    {
        var shard = await context.RequireShard();
        if (shard is null)
        {
            return null;
        }

        // TODO: replace with real shard names, once shard have names
        var shardName = LocalizedString.CreateInvariant(shard.Id);

        if (!shard.Variants.TryGetValue(variant, out var variantObject))
        {
            context.Report(new MissingShardVariantDiagnostic(
                ShardName: shardName,
                ShardId: shard.Id,
                Variant: Const.OriginalShardVariant
            ));
            return null;
        }
        return (shard, variantObject);
    }

    public static async Task<(ShardInfo shard, TVariant variant)?> RequireShardVariant<TVariant>(
        this IRequirementContext<IRequirement> context,
        string variant
    )
    {
        var pair = await context.RequireShardVariant(variant);
        if (pair is null)
        {
            return null;
        }

        var (shard, variantObject) = pair.Value;

        if (variantObject.Value is not TVariant typedVariant)
        {
            context.Report(new IncompatibleRequirementDiagnostic(
                context.RequirementType,
                variantObject.Type
            ));
            return null;
        }

        return (shard, typedVariant);
    }
}
