using System.Threading;
using System.Threading.Tasks;

namespace Kafe;

public interface IShardAnalyzer
{
    ValueTask<ShardAnalysis> Analyze(string tempPath, string? mimeType, CancellationToken token = default);
}
