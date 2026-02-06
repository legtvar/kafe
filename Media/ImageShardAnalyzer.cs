using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Services;
using Kafe.Media.Services;

namespace Kafe.Media;

public class ImageShardAnalyzer(
    IImageService imageService,
    StorageService storageService
) : IShardAnalyzer
{

    public async ValueTask<ShardAnalysis> Analyze(ShardAnalyzerContext context, CancellationToken token = default)
    {
        var shardPath = storageService.GetAbsolutePath(context.ShardUri);
        var imageInfo = await imageService.GetInfo(shardPath, token);

        return new(
            payload: imageInfo,
            fileExtension: imageInfo.FileExtension
        );
    }
}
