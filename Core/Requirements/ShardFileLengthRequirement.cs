using System.Threading.Tasks;
using Kafe.Core.Diagnostics;

namespace Kafe.Core.Requirements;

public record ShardFileLengthRequirement(
    int? Min,
    int? Max
) : IRequirement
{
    public static string Moniker => "shard-file-length";
}

public sealed class ShardFileLengthRequirementHandler : ShardRequirementHandlerBase<ShardFileLengthRequirement>
{
    public override ValueTask Handle(
        IShardRequirementContext<ShardFileLengthRequirement> context
    )
    {
        if (context.Requirement.Min is null && context.Requirement.Max is null)
        {
            // TODO: warn about the uselessness of the user's doing.
            return ValueTask.CompletedTask;
        }

        if (context.Shard.FileLength < 0)
        {
            context.Report(new CorruptedShardDiagnostic(context.Shard.Name, context.Shard.Id));
            return ValueTask.CompletedTask;
        }

        if (context.Requirement.Min is not null
            && context.Shard.FileLength < context.Requirement.Min.Value)
        {
            context.Report(new ShardTooSmallDiagnostic(
                context.Shard.Name,
                context.Shard.Id,
                context.Requirement.Min.Value
            ));
        }

        if (context.Requirement.Max is not null
            && context.Shard.FileLength > context.Requirement.Max.Value)
        {
            context.Report(new ShardTooLargeDiagnostic(
                context.Shard.Name,
                context.Shard.Id,
                context.Requirement.Max.Value
            ));
        }

        return ValueTask.CompletedTask;
    }
}
