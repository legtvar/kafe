using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Kafe.Media;

public static class Blender
{
    public static string? FindExecutable()
    {
        char separator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';
        var envPath = Environment.GetEnvironmentVariable("PATH");
        if (envPath is null)
        {
            return null;
        }

        foreach (var path in envPath.Split(separator, StringSplitOptions.RemoveEmptyEntries))
        {
            var dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                foreach (var file in dir.EnumerateFiles("blender*"))
                {
                    if (file.Name == "blender")
                    {
                        return file.FullName;
                    }
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && file.Name == "blender.exe")
                    {
                        return file.FullName;
                    }
                }
            }
        }
        return null;
    }
    public static async Task<bool> RunBlenderCommand(string arguments)
    {
        using var pro = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = FindExecutable(),
                Arguments = arguments,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };
        pro.Start();
        await pro.WaitForExitAsync();
        bool success = pro.ExitCode == 0;
        return success;
    }
}