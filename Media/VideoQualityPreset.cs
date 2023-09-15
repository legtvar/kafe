using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public enum VideoQualityPreset
{
    Invalid = 0,
    Original = 1,
    SD = 2,
    HD = 3,
    FullHD = 4
}

public static class VideoQualityPresetExtensions
{
    public static string? ToFileName(this VideoQualityPreset preset)
    {
        return preset switch
        {
            VideoQualityPreset.Original => "original",
            VideoQualityPreset.SD => "sd",
            VideoQualityPreset.HD => "hd",
            VideoQualityPreset.FullHD => "fullhd",
            _ => null
        };
    }
}
