using System;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Services;
using Kafe.Media.Services;

namespace Kafe.Media;

public class MediaShardAnalyzer(
    IMediaService mediaService,
    StorageService storageService
) : IShardAnalyzer
{
    public async ValueTask<ShardAnalysis> Analyze(ShardAnalyzerContext context, CancellationToken token = default)
    {
        if (context.MimeType != Const.MatroskaMimeType && context.MimeType != Const.Mp4MimeType)
        {
            throw new ArgumentException($"Only '{Const.MatroskaMimeType}' and '{Const.Mp4MimeType}' video container " +
                $"formats are supported.");
        }

        var originalFileExtension = context.MimeType == Const.MatroskaMimeType
            ? Const.MatroskaFileExtension
            : Const.Mp4FileExtension;

        var shardPath = storageService.GetAbsolutePath(context.ShardUri);
        var mediaInfo = await mediaService.GetInfo(shardPath, token);

        return new(
            payload: mediaInfo,
            fileExtension: originalFileExtension
        );
    }
}
