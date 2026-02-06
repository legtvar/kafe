using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe;

public interface IShardAnalyzer
{
    ValueTask<ShardAnalysis> Analyze(ShardAnalyzerContext context, CancellationToken token = default);
}
