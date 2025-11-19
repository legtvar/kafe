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
    Sd = 2,
    Hd = 3,
    FullHd = 4
}

public static class VideoQualityPresetExtensions
{
    public static string? ToFileName(this VideoQualityPreset preset)
    {
        return preset switch
        {
            VideoQualityPreset.Original => "original",
            VideoQualityPreset.Sd => "sd",
            VideoQualityPreset.Hd => "hd",
            VideoQualityPreset.FullHd => "fullhd",
            _ => null
        };
    }
    
    public static int ToHeight(this VideoQualityPreset preset)
    {
        return preset switch
        {
            VideoQualityPreset.Sd => 480,
            VideoQualityPreset.Hd => 720,
            VideoQualityPreset.FullHd => 1080,
            _ => -1
        };
    }
}
