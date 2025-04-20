using System.Threading.Tasks;
using Kafe.Core.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe.Core.Requirements;

public static class RequirementContextExtensions
{
    public static async Task<IShard?> RequireShard(this IRequirementContext<IRequirement> context)
    {
        if (context.Object.Value is not ShardReferenceProperty shardRef)
        {
            context.Report(new IncompatibleRequirementDiagnostic(
                context.RequirementType,
                context.Object.Type
            ));
            return null;
        }

        var repo = context.ServiceProvider.GetRequiredService<IRepository<IShard>>();
        var shard = await repo.Read(shardRef.ShardId, context.CancellationToken);
        if (shard.HasDiagnostic)
        {
            context.Report(shard.Diagnostic);
            return shard.Value;
        }

        return shard.Value;
    }

    public static async Task<(IShard shard, KafeObject variant)?> RequireShardVariant(
        this IRequirementContext<IRequirement> context,
        string variant
    )
    {
        var shard = await context.RequireShard();
        if (shard is null)
        {
            return null;
        }

        if (!shard.Variants.TryGetValue(variant, out var variantObject))
        {
            context.Report(new MissingShardVariantDiagnostic(
                ShardName: shard.Name,
                ShardId: shard.Id,
                Variant: Const.OriginalShardVariant
            ));
            return null;
        }
        return (shard, variantObject);
    }

    public static async Task<(IShard shard, TVariant variant)?> RequireShardVariant<TVariant>(
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
