using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Kafe.Media;

public static class FFmpeg
{
    public static string? FindExecutable()
    {
        char separator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';
        var envPath = Environment.GetEnvironmentVariable("PATH");
        if (envPath is null)
        {
            return null;
        }

        foreach (var path in envPath.Split(separator))
        {
            var dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                foreach (var file in dir.EnumerateFiles("ffmpeg*"))
                {
                    return file.FullName;
                }
            }
        }
        return null;
    }
}
