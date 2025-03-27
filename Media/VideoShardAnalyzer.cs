using System;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Media.Services;

namespace Kafe.Media;

public class VideoShardAnalyzer : IShardAnalyzer
{
    private readonly IMediaService mediaService;

    public VideoShardAnalyzer(IMediaService mediaService)
    {
        this.mediaService = mediaService;
    }

    public async ValueTask<ShardAnalysis> Analyze(string tempPath, string? mimeType, CancellationToken token = default)
    {
        if (mimeType != Const.MatroskaMimeType && mimeType != Const.Mp4MimeType)
        {
            throw new ArgumentException($"Only '{Const.MatroskaMimeType}' and '{Const.Mp4MimeType}' video container " +
                $"formats are supported.");
        }

        var originalFileExtension = mimeType == Const.MatroskaMimeType
            ? Const.MatroskaFileExtension
            : Const.Mp4FileExtension;

        var mediaInfo = await mediaService.GetInfo(tempPath, token);

        return new(
            shardMetadata: mediaInfo,
            fileExtension: originalFileExtension
        );
    }
}
