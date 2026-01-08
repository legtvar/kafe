using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe;

public class ShardAnalysisFactory(
    KafeTypeRegistry typeRegistry,
    IServiceProvider serviceProvider
)
{
    public async ValueTask<ShardAnalysis> Create(
        KafeType shardType,
        string filePath,
        string? mimeType,
        CancellationToken ct = default
    )
    {
        var typeMetadata = typeRegistry.RequireMetadata(shardType);
        var shardTypeMetadata = typeMetadata.RequireExtension<ShardPayloadTypeMetadata>();
        foreach (var analyzerType in shardTypeMetadata.AnalyzerTypes)
        {
            var analyzer = serviceProvider.GetService(analyzerType)
                ?? ActivatorUtilities.CreateInstance(serviceProvider, analyzerType);
            if (analyzer is not IShardAnalyzer shardAnalyzer)
            {
                throw new InvalidOperationException(
                    $"Could not obtain an instance of the "
                    + $"'{analyzerType.FullName}' shard analyzer."
                );
            }

            var analysis = await shardAnalyzer.Analyze(filePath, mimeType, ct);
            if (analysis.IsSuccessful)
            {
                if (string.IsNullOrWhiteSpace(analysis.ShardAnalyzerName))
                {
                    analysis.ShardAnalyzerName = analyzerType.FullName;
                }

                if (analysis.Payload.GetType() != typeMetadata.DotnetType)
                {
                    throw new InvalidOperationException(
                        $"The '{analyzer.GetType().FullName}' analyzer"
                        + $" produced a shard of type '{analysis.Payload.GetType().FullName}' "
                        + $"but '{typeMetadata.DotnetType.FullName}' was required. "
                        + "The shard type's analyzers are likely misconfigured."
                    );
                }

                return analysis;
            }
        }

        return ShardAnalysis.Invalid;
    }
}
