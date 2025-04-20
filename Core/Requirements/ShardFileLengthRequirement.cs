using System.Threading.Tasks;
using Kafe.Core.Diagnostics;

namespace Kafe.Core.Requirements;

public record ShardFileLengthRequirement(
    int? MinLength,
    int? MaxLength
) : IRequirement
{
    public static string Moniker { get; } = "shard-file-length";
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

        if (shard.FileLength < 0)
        {
            context.Report(new CorruptedShardDiagnostic(shard.Name, shard.Id));
            return;
        }

        if (context.Requirement.MinLength is not null
            && shard.FileLength < context.Requirement.MinLength.Value)
        {
            context.Report(new ShardTooSmallDiagnostic(shard.Name, shard.Id, context.Requirement.MinLength.Value));
        }

        if (context.Requirement.MaxLength is not null
            && shard.FileLength > context.Requirement.MaxLength.Value)
        {
            context.Report(new ShardTooLargeDiagnostic(shard.Name, shard.Id, context.Requirement.MaxLength.Value));
        }
    }
}
