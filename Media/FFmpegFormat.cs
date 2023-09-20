using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public static class FFmpegFormat
{
    public const string FallbackContentType = "application/octet-stream";

    // TODO: steal mime types from Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider
    private static readonly ImmutableDictionary<string, ExtensionMimePair> FormatNameMap
        = new Dictionary<string, ExtensionMimePair>(StringComparer.OrdinalIgnoreCase)
        {
            ["matroska,webm"]
                = new(".mkv", "video/x-matroska"),
            ["mp4"]
                = new(".mp4", "video/mp4"),
            ["m4v"]
                = new(".m4v", "video/mp4"),
            ["mov,mp4,m4a,3gp,3g2,mj2"]
                = new(".mp4", "video/mp4"),
            ["srt"]
                = new(".srt", "application/x-subrip"),
            ["ass"]
                = new(".ass", "text/plain"),
            ["ssa"]
                = new(".ssa", "text/plain"),
            ["webvtt"]
                = new(".vtt", "text/vtt"),
            ["3g2"]
                = new(".3g2", "video/3gpp2"),
            ["avi"]
                = new(".avi", "video/x-msvideo"),
            ["asf"]
                = new(".asf", "video/x-ms-asf"),
            ["mpeg"]
                = new(".mpg", "video/mpeg"),
            ["flv"]
                = new(".flv", "video/x-flv"),
        }.ToImmutableDictionary();

    public static string? GetFileExtension(string fileFormat)
    {
        return FormatNameMap.GetValueOrDefault(fileFormat).FileExtension;
    }

    // TODO: Figure out a way to handle the webm format correctly
    public static string? GetMimeType(string extension, string fileFormat)
    {
        if (extension == ".mkv" && fileFormat == "matroska,webm")
        {
            return "video/webm";
        }

        return FormatNameMap.GetValueOrDefault(fileFormat).MimeType;
    }

    public static string? GetMimeType(string fileFormat)
    {
        return FormatNameMap.GetValueOrDefault(fileFormat).MimeType;
    }

    private readonly record struct ExtensionMimePair(string FileExtension, string MimeType);
}
