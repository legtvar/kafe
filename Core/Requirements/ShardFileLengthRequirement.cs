using System.Threading.Tasks;
using Kafe.Core.Diagnostics;

namespace Kafe.Core.Requirements;

public record ShardFileLengthRequirement(
    int? Min,
    int? Max
) : IRequirement
{
    public static string Moniker { get; } = "shard-file-length";
}

public sealed class ShardFileLengthRequirementHandler : ShardRequirementHandlerBase<ShardFileLengthRequirement>
{
    public override ValueTask Handle(
        IRequirementContext<ShardFileLengthRequirement> context,
        IShard shard
    )
    {
        if (context.Requirement.Min is null && context.Requirement.Max is null)
        {
            // TODO: warn about the uselessness of the user's doing.
            return ValueTask.CompletedTask;
        }

        if (shard.FileLength < 0)
        {
            context.Report(new CorruptedShardDiagnostic(shard.Name, shard.Id));
            return ValueTask.CompletedTask;
        }

        if (context.Requirement.Min is not null
            && shard.FileLength < context.Requirement.Min.Value)
        {
            context.Report(new ShardTooSmallDiagnostic(shard.Name, shard.Id, context.Requirement.Min.Value));
        }

        if (context.Requirement.Max is not null
            && shard.FileLength > context.Requirement.Max.Value)
        {
            context.Report(new ShardTooLargeDiagnostic(shard.Name, shard.Id, context.Requirement.Max.Value));
        }
        
        return ValueTask.CompletedTask;
    }
}
