using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public record SubtitlesInfo(
    string FileExtension,
    string MimeType,
    string? Language,
    string Codec,
    long Bitrate,
    bool IsCorrupted
) : IShardPayload
{
    public static string Moniker => "subtitles";

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Subtitles"),
        (Const.CzechCulture, "Titulky")
    );
}
