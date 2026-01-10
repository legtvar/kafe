using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Kafe.Core.Diagnostics;

namespace Kafe.Core.Requirements;

public record ShardMimeTypeRequirement(
    ImmutableArray<string> Include,
    ImmutableArray<string> Exclude
) : IRequirement
{
    public static string Moniker => "shard-mime-type";
}

public class ShardMimeTypeRequirementHandler : ShardRequirementHandlerBase<ShardMimeTypeRequirement>
{
    public override ValueTask Handle(IShardRequirementContext<ShardMimeTypeRequirement> context)
    {
        if (context.Requirement.Include.IsDefaultOrEmpty && context.Requirement.Exclude.IsDefaultOrEmpty)
        {
            return ValueTask.CompletedTask;
        }

        var allowedTypes = context.Requirement.Include.Except(context.Requirement.Exclude).ToImmutableArray().Sort();

        if (!allowedTypes.Contains(context.Shard.MimeType))
        {
            context.Report(new ShardMimeTypeNotAllowedDiagnostic(
                context.Shard.Id,
                context.Shard.Name,
                context.Shard.MimeType,
                allowedTypes
            ));
            return ValueTask.CompletedTask;
        }

        return ValueTask.CompletedTask;
    }
}

