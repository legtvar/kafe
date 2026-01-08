using System.Threading.Tasks;
using Kafe.Core.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe.Core.Requirements;

public static class RequirementContextExtensions
{
    public static async Task<IShard?> RequireShard(this IRequirementContext<IRequirement> context)
    {
        if (context.Target.Value is not ShardReference shardRef)
        {
            context.Report(new IncompatibleRequirementDiagnostic(
                context.RequirementType,
                context.Target.Type
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

    public static async Task<(IShard shard, TMetadata metadata)?> RequireShardMetadata<TMetadata>(
        this IRequirementContext<IRequirement> context
    )
    {
        var shard = await context.RequireShard();
        if (shard is null)
        {
            return null;
        }

        if (shard.Payload.Value is not TMetadata metadata)
        {
            context.Report(new IncompatibleRequirementDiagnostic(
                context.RequirementType,
                shard.Payload.Type
            ));
            return null;
        }

        return (shard, metadata);
    }

    public static TMetadata? RequireShardMetadata<TMetadata>(
        this IShardRequirementContext<IRequirement> context
    )
    {
        if (context.Shard.Payload.Value is not TMetadata metadata)
        {
            context.Report(new IncompatibleRequirementDiagnostic(
                context.RequirementType,
                context.Shard.Payload.Type
            ));
            return default;
        }

        return metadata;
    }
}
