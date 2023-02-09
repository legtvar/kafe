using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Media;

public interface IMediaService
{
    public const string SDFileName = "sd.mp4";
    public const string HDFileName = "hd.mp4";
    public const string FullHDFileName = "fullhd.mp4";
    public const string OriginalFileName = "original";

    Task<MediaInfo> GetInfo(string filePath);

    Task Save(Hrib hrib, Stream data, CancellationToken cancellationToken = default);

    Stream? Load(Hrib hrib, VideoQualityPreset preset = VideoQualityPreset.Original);

    ImmutableArray<VideoQualityPreset> GetAvailablePresets(Hrib hrib);

    Task<bool> ConvertToPreset(Hrib hrib, VideoQualityPreset preset, CancellationToken cancellationToken = default);
}
