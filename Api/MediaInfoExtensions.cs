using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public static class MediaInfoExtensions
{
    public const string FallbackContentType = "application/octet-stream";

    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    private static readonly ImmutableDictionary<string, (string extension, string mime)> FormatNameMap
        = ImmutableDictionary.CreateRange(new KeyValuePair<string, (string extension, string mime)>[]
        {
            new("matroska,webm", (".mkv", "video/x-matroska")),
            new("mp4", (".mp4", "video/mp4")),
            new("m4v", (".m4v", "video/mp4")),
            new("mov,mp4,m4a,3gp,3g2,mj2", (".mp4", "video/mp4"))
        });

    public static string GetMimeType(this MediaInfo media)
    {
        if (media is null
            || string.IsNullOrEmpty(media.FileExtension)
            || string.IsNullOrEmpty(media.FormatName))
        {
            return FallbackContentType;
        }

        if (FormatNameMap.TryGetValue(media.FormatName, out var pair))
        {
            return pair.mime;
        }

        if (ContentTypeProvider.TryGetContentType(media.FileExtension, out var contentType))
        {
            return contentType;
        }

        return FallbackContentType;
    }
}
