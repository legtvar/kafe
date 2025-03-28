using System;
using System.Collections.Generic;
using System.Linq;

namespace Kafe;

public class FileExtensionMimeMap
{
    public Dictionary<string, List<string>> FileExtensionToMime { get; set; }
        = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, List<string>> MimeToFileExtension { get; set; }
        = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

    public FileExtensionMimeMap Add(string fileExtension, string mimeType)
    {
        fileExtension = fileExtension.ToLower();
        if (!fileExtension.StartsWith('.'))
        {
            throw new ArgumentException("A file extension must start with '.'.");
        }

        if (!FileExtensionToMime.TryGetValue(fileExtension, out var mimeList))
        {
            mimeList = [];
            FileExtensionToMime.Add(fileExtension, mimeList);
        }

        if (!mimeList.Contains(mimeType))
        {
            mimeList.Add(mimeType);
        }

        if (!MimeToFileExtension.TryGetValue(mimeType, out var fileExtensionList))
        {
            fileExtensionList = [];
            MimeToFileExtension.Add(mimeType, fileExtensionList);
        }

        if (!fileExtensionList.Contains(fileExtension))
        {
            fileExtensionList.Add(fileExtension);
        }

        return this;
    }

    public string? GetFirstFileExtensionFor(string mimeType)
    {
        if (MimeToFileExtension.TryGetValue(mimeType, out var fileExtensions) && fileExtensions.Count > 0)
        {
            return fileExtensions.First();
        }

        return null;
    }

    public string? GetFirstMimeTypeFor(string fileExtension)
    {
        if (FileExtensionToMime.TryGetValue(fileExtension, out var mimeTypes) && mimeTypes.Count > 0)
        {
            return mimeTypes.First();
        }

        return null;
    }
}
