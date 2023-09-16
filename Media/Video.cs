using System.Collections.Immutable;
using System.Linq;

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
            presets.Add(VideoQualityPreset.FullHD);
        }

        if (maxHeight >= 720)
        {
            presets.Add(VideoQualityPreset.HD);
        }

        if (maxHeight >= 480)
        {
            presets.Add(VideoQualityPreset.SD);
        }

        // Reverse the presets so that the smallest resolution is first.
        presets.Reverse();

        return presets.ToImmutable();
    }
}
