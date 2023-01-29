using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public interface IMediaService
{
    public const string SDFileName = "sd.mp4";
    public const string HDFileName = "hd.mp4";
    public const string FullHDFileName = "fullhd.mp4";
    public const string OriginalFileName = "original";

    Task<MediaInfo> GetInfo(string filePath);

    Task Save(Hrib)
}
