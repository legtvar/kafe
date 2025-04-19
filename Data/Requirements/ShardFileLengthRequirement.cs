using System.Threading.Tasks;
using Kafe.Core.Diagnostics;
using Kafe.Data.Diagnostics;

namespace Kafe.Data.Requirements;

public record ShardFileLengthRequirement(
    int? MinLength,
    int? MaxLength
) : IRequirement
{
    public static string Name { get; } = "shard-file-length";
}

public sealed class ShardFileLengthRequirementHandler : RequirementHandlerBase<ShardFileLengthRequirement>
{
    public override async ValueTask Handle(IRequirementContext<ShardFileLengthRequirement> context)
    {
        if (context.Requirement.MinLength is null && context.Requirement.MaxLength is null)
        {
            // TODO: warn about the uselessness of the user's doing.
            return;
        }

        var shard = await context.RequireShard();
        if (shard is null)
        {
            return;
        }

        // TODO: replace with real shard name
        var shardName = LocalizedString.CreateInvariant(shard.Id);

        if (shard.FileLength is null)
        {
            context.Report(new CorruptedShardDiagnostic(shardName, shard.Id));
            return;
        }

        if (context.Requirement.MinLength is not null
            && shard.FileLength.Value < context.Requirement.MinLength.Value)
        {
            context.Report(new ShardTooSmallDiagnostic(shardName, shard.Id, context.Requirement.MinLength.Value));
        }

        if (context.Requirement.MaxLength is not null
            && shard.FileLength.Value > context.Requirement.MaxLength.Value)
        {
            context.Report(new ShardTooLargeDiagnostic(shardName, shard.Id, context.Requirement.MaxLength.Value));
        }
    }
}
