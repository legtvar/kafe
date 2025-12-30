using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Mate;

public class BlendShardAnalyzer : IShardAnalyzer
{
    public ValueTask<ShardAnalysis> Analyze(string tempPath, string? mimeType, CancellationToken token = default)
    {
        // TODO: This is where the analysis of the blend file should happen (i.e. at the very least this method
        //       should make sure that the file is really a valid Blender scene and is not corrupted.)
        return ValueTask.FromResult(new ShardAnalysis()
        {
            IsSuccessful = true,
            FileExtension = ".blend",
            MimeType = "application/x-blender",
            ShardMetadata = new BlendInfo(
                FileExtension: ".blend",
                MimeType: "application/x-blender"
            )
        });
    }
}
