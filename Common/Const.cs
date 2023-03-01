using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe;

public static class Const
{
    public const string OriginalShardVariant = "original";
    public const string InvalidPath = "invalid";
    public const long ShardSizeLimit = 4_294_967_296;

    public const string MatroskaMimeType = "video/x-matroska";
    public const string MatroskaFileExtension = ".mkv";

    public const string Mp4MimeType = "video/mp4";
    public const string Mp4FileExtension = ".mp4";

    public static readonly LocalizedString UnknownAuthor
        = LocalizedString.Create(
            (CultureInfo.InvariantCulture, "Unknown author"),
            (CultureInfo.CreateSpecificCulture("cs"), "Neznámý autor"));

    public static readonly LocalizedString UnknownProjectGroup
        = LocalizedString.Create(
            (CultureInfo.InvariantCulture, "Unknown project group"),
            (CultureInfo.CreateSpecificCulture("cs"), "Neznámá skupina projektů"));
}
