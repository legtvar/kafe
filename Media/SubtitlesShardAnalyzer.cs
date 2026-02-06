using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Services;
using Kafe.Media.Services;

namespace Kafe.Media;

public class SubtitlesShardAnalyzer(
    IMediaService mediaService,
    StorageService storageService
) : IShardAnalyzer
{

    public async ValueTask<ShardAnalysis> Analyze(ShardAnalyzerContext context, CancellationToken token = default)
    {
        var path = storageService.GetAbsolutePath(context.ShardUri);
        var mediaInfo = await mediaService.GetInfo(path, token);

        if (mediaInfo.SubtitleStreams.IsDefaultOrEmpty)
        {
            throw new ArgumentException("The file contains no subtitle streams.");
        }

        if (mediaInfo.SubtitleStreams.Length > 1)
        {
            throw new ArgumentException("The file contains more than one subtitle stream.");
        }

        var ssInfo = mediaInfo.SubtitleStreams.Single();
        var info = new SubtitlesInfo(
            FileExtension: FFmpegFormat.GetFileExtension(mediaInfo.FormatName) ?? Const.InvalidFileExtension,
            MimeType: FFmpegFormat.GetMimeType(mediaInfo.FormatName) ?? Const.InvalidMimeType,
            Language: ssInfo.Language,
            Codec: ssInfo.Codec,
            Bitrate: ssInfo.Bitrate,
            IsCorrupted: mediaInfo.IsCorrupted);

        return new(
            payload: info,
            fileExtension: mediaInfo.FileExtension
        );
    }
}
