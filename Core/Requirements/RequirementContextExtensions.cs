using System.Threading.Tasks;
using Kafe.Core.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe.Core.Requirements;

public static class RequirementContextExtensions
{
    extension(IRequirementContext<IRequirement> ctx)
    {
        public void ReportIncompatible()
        {
            ctx.Report(
                new IncompatibleRequirementDiagnostic(
                    ctx.Requirement.GetType(),
                    ctx.Target.GetType()
                )
            );
        }

        public async Task<IShard?> RequireShard()
        {
            if (ctx.Target is not ShardReference shardRef)
            {
                ctx.ReportIncompatible();
                return null;
            }

            var readOp = ctx.ServiceProvider.GetRequiredService<IReadById<IShard>>();
            var shard = await readOp.Read(shardRef.ShardId, ctx.CancellationToken);
            if (shard is null)
            {
                ctx.Report(new NotFoundDiagnostic(typeof(IShard), shardRef.ShardId));
                return null;
            }

            return shard;
        }

        public async Task<(IShard shard, TPayload payload)?> RequireShardPayload<TPayload>()
        {
            var shard = await ctx.RequireShard();
            if (shard is null)
            {
                return null;
            }

            if (shard.Payload.Value is not TPayload payload)
            {
                ctx.ReportIncompatible();
                return null;
            }

            return (shard, payload);
        }
    }

    extension(IShardRequirementContext<IRequirement> shardCtx)
    {
        public TPayload? RequireShardPayload<TPayload>()
        {
            if (shardCtx.Shard.Payload.Value is not TPayload payload)
            {
                shardCtx.ReportIncompatible();
                return default;
            }

            return payload;
        }
    }
}
