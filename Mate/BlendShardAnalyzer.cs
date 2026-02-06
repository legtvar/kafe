using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Mate;

public class BlendShardAnalyzer
    : IShardAnalyzer
{
    public ValueTask<ShardAnalysis> Analyze(ShardAnalyzerContext context, CancellationToken token = default)
    {
        // TODO: This is where the analysis of the blend file should happen (i.e. at the very least this method
        //       should make sure that the file is really a valid Blender scene and is not corrupted.)
        return ValueTask.FromResult(new ShardAnalysis()
        {
            IsSuccessful = true,
            FileExtension = Const.BlendFileExtension,
            MimeType = Const.BlendMimeType,
            Payload = new BlendInfo(
                FileExtension: Const.BlendFileExtension,
                MimeType: Const.BlendMimeType,
                Tests: null,
                Error: null
            )
        });
    }
}
