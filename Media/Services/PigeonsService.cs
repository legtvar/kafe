using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kafe.Media.Services;

public class PigeonsCoreService
{
    private readonly ILogger<PigeonsCoreService> logger;

    private const string pigeonsTestOutputName = "pigeons_test_result";
    private const string pigeonsTestOutputExtension = "json";
    public PigeonsCoreService(ILogger<PigeonsCoreService>? logger = null)
    {
        this.logger = logger ?? NullLogger<PigeonsCoreService>.Instance;
    }
    

    public static string? FindPigeonsManagerPath()
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
                foreach (var file in dir.EnumerateFiles())
                {
                    if (file.Name == "headless.py")
                    {
                        return file.FullName;
                    }
                }
            }
        }
        return null;
    }

    public static string GetPigeonsTestOutputPath(Hrib id, string filePath)
    {
        string? filePathDirectory = Path.GetDirectoryName(filePath);
        if (filePathDirectory is null)
        {
            throw new InvalidOperationException("Failed to find pigeons output file path directory .");
        }
        string hashSuffix = id.ToString() + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        return Path.Combine(filePathDirectory, $"{pigeonsTestOutputName}_{hashSuffix}.{pigeonsTestOutputExtension}");
    }

    public static string GetHomeworkType()
    {
        return "homework5materials";
    }


    public static string GetPigeonsTestCommand(Hrib id, string filePath, string type)
    {
        string args = string.Join(" ", new[]
        {
            "--background",
            "--python",$"\"{FindPigeonsManagerPath()}\"",
            "--",
            "test",
            $"--hw={type}",
            $"--homework-file=\"{filePath}\"",
            "--output-to-file", $"\"{GetPigeonsTestOutputPath(id, filePath)}\""
        });
        return args;
    }

    public async Task<bool> RunPigeonsTest(Hrib id, string shardPath)
    {
        string arguments = GetPigeonsTestCommand(id, shardPath, GetHomeworkType());
        return await Blender.RunBlenderCommand(arguments);
    }
}