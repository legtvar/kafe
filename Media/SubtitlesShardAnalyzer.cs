using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Media.Services;

namespace Kafe.Media;

public class SubtitlesShardAnalyzer : IShardAnalyzer
{
    private readonly IMediaService mediaService;

    public SubtitlesShardAnalyzer(IMediaService mediaService)
    {
        this.mediaService = mediaService;
    }

    public async ValueTask<ShardAnalysis> Analyze(string tempPath, string? mimeType, CancellationToken token = default)
    {
        var mediaInfo = await mediaService.GetInfo(tempPath, token);

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
            shardMetadata: info,
            fileExtension: mediaInfo.FileExtension
        );
    }
}
