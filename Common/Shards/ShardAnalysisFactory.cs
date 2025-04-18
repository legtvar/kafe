using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe;

public class ShardAnalysisFactory
{
    private readonly ShardTypeRegistry shardTypes;
    private readonly IServiceProvider services;

    public ShardAnalysisFactory(
        ShardTypeRegistry shardTypes,
        IServiceProvider services
    )
    {
        this.shardTypes = shardTypes;
        this.services = services;
    }

    public async ValueTask<ShardAnalysis> Create(
        KafeType shardType,
        string filePath,
        string? mimeType,
        CancellationToken ct = default
    )
    {
        var shardTypeMetadata = shardTypes.Metadata.GetValueOrDefault(shardType)
            ?? throw new ArgumentException($"Shard type '{shardType}' could not be recognized.");

        foreach (var analyzerType in shardTypeMetadata.AnalyzerTypes)
        {
            var analyzer = services.GetService(analyzerType)
                ?? ActivatorUtilities.CreateInstance(services, analyzerType);
            if (analyzer is not IShardAnalyzer shardAnalyzer)
            {
                throw new InvalidOperationException($"Could not obtain an instance of the "
                    + $"'{analyzerType.FullName}' shard analyzer.");
            }

            var analysis = await shardAnalyzer.Analyze(filePath, mimeType, ct);
            if (analysis.IsSuccessful)
            {
                if (string.IsNullOrWhiteSpace(analysis.ShardAnalyzerName))
                {
                    analysis.ShardAnalyzerName = analyzerType.FullName;
                }

                return analysis;
            }
        }

        return ShardAnalysis.Invalid;
    }
}
