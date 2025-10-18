using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Pigeons.Services;

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
                    if (file.Name == "blender-headless" || file.Name == "blender")
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
    public static async Task<BlenderProcessOutput> RunBlenderCommand(string arguments, int? timeout = null)
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
        if (timeout == null)
        {
            await pro.WaitForExitAsync();
        } else
        {
            Task task = pro.WaitForExitAsync();
            Task result = await Task.WhenAny(task, Task.Delay(timeout.Value));
            if (result != task)
            {
                try
                {
                    pro.Kill();
                }
                catch (Exception)
                {
                    // Ignore exceptions from Kill, process might have already exited.
                }
                return new BlenderProcessOutput(false, "Blender process timed out.");
            }
        }
        if (pro.ExitCode == 0)
        {
            return new BlenderProcessOutput(true);
        }
        string message = await pro.StandardError.ReadToEndAsync();
        return new BlenderProcessOutput(false, "Blender process exited with code " + pro.ExitCode + ": " + message);
    }
}