using System.IO;
using System.Threading.Tasks;

namespace Kafe;

public interface IShardAnalyzer
{
    ValueTask<object> Analyze(Stream stream, string? mimeType);
}
