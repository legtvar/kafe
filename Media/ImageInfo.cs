using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public record ImageInfo(
    string FileExtension,
    string MimeType,
    string FormatName,
    int Width,
    int Height,
    bool IsCorrupted = false
)
{
    public static ImageInfo Invalid { get; } = new(
        FileExtension: Const.InvalidFileExtension,
        MimeType: Const.InvalidMimeType,
        FormatName: Const.InvalidFormatName,
        Width: -1,
        Height: -1,
        IsCorrupted: true
    );
}
