using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageSharpInfo = SixLabors.ImageSharp.ImageInfo;

namespace Kafe.Media.Services;

public class ImageSharpService : IImageService
{
    public async Task<ImageInfo> GetInfo(string filePath, CancellationToken token = default)
    {
        var info = await Image.IdentifyAsync(filePath, token);
        return GetImageInfo(info);
    }

    public async Task<ImageInfo> GetInfo(Stream stream, CancellationToken token = default)
    {
        var info = await Image.IdentifyAsync(stream, token);
        return GetImageInfo(info);
    }

    private ImageInfo GetImageInfo(ImageSharpInfo info)
    {
        return new ImageInfo(
            FileExtension: GetFileExtension(info.Metadata.DecodedImageFormat)
                ?? Const.InvalidFileExtension,
            FormatName: info.Metadata.DecodedImageFormat?.Name
                ?? Const.InvalidFileExtension,
            MimeType: info.Metadata.DecodedImageFormat?.DefaultMimeType
                ?? Const.InvalidMimeType,
            Width: info.Width,
            Height: info.Height);
    }

    private string? GetFileExtension(IImageFormat? format)
    {
        if (format is null)
        {
            return null;
        }

        var extension = format.FileExtensions.FirstOrDefault();
        if (extension is null)
        {
            return null;
        }

        return extension.StartsWith('.') ? extension : $".{extension}";
    }
}
