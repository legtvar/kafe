using System.Collections.Immutable;
using System.Linq;
using Kafe.Media.Services;

namespace Kafe.Media;

public static class Video
{
    public static ImmutableArray<VideoQualityPreset> GetApplicablePresets(MediaInfo media)
    {
        if (media.VideoStreams.IsEmpty)
        {
            return ImmutableArray<VideoQualityPreset>.Empty;
        }

        var maxHeight = media.VideoStreams.Max(v => v.Height);
        var presets = ImmutableArray.CreateBuilder<VideoQualityPreset>();
        if (maxHeight >= 1080)
        {
            presets.Add(VideoQualityPreset.FullHd);
        }

        if (maxHeight >= 720)
        {
            presets.Add(VideoQualityPreset.Hd);
        }

        if (maxHeight >= 480)
        {
            presets.Add(VideoQualityPreset.Sd);
        }

        // Reverse the presets so that the smallest resolution is first.
        presets.Reverse();

        return presets.ToImmutable();
    }

    public static VideoQualityPreset GetPresetFromFileName(string fileName)
    {
        return fileName switch
        {
            Const.OriginalShardVariant => VideoQualityPreset.Original,
            IMediaService.FullHdFileName => VideoQualityPreset.FullHd,
            IMediaService.HdFileName => VideoQualityPreset.Hd,
            IMediaService.SdFileName => VideoQualityPreset.Sd,
            _ => VideoQualityPreset.Invalid
        };
    }
}
