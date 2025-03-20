using System.Threading;
using System.Threading.Tasks;
using Kafe.Media.Services;

namespace Kafe.Media;

public class ImageShardAnalyzer : IShardAnalyzer
{
    private readonly IImageService imageService;

    public ImageShardAnalyzer(IImageService imageService)
    {
        this.imageService = imageService;
    }

    public async ValueTask<ShardAnalysis> Analyze(string tempPath, string? mimeType, CancellationToken token = default)
    {
        var imageInfo = await imageService.GetInfo(tempPath, token);

        return new(
            FileExtension: imageInfo.FileExtension,
            ShardMetadata: imageInfo
        );
    }
}
