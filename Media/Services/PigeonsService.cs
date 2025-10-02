using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;

namespace Kafe.Media.Services;

public class PigeonsCoreService
{
    private readonly ILogger<PigeonsCoreService> logger;

    private const string pigeonsTestOutputName = "pigeons_test_result";
    private const string pigeonsTestOutputExtension = "json";
    private const int pigeonsTestTimeoutMs = 100000;

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
            "--python-exit-code", "1",
            "--python",$"\"{FindPigeonsManagerPath()}\"",
            "--",
            "test",
            $"--hw={type}",
            $"--homework-file=\"{filePath}\"",
            "--output-to-file", $"\"{GetPigeonsTestOutputPath(id, filePath)}\""
        });
        return args;
    }
    
    public class PigeonsTestResult : Dictionary<int, PigeonsTestResultDetails>
    {
    }

    public class PigeonsTestResultDetails
    {
        public string? label { get; set; }
        public string? state { get; set; }
        public string? datablock { get; set; }
        public string? message { get; set; }
        public string? traceback { get; set; }
    };

    public async Task<BlendInfo> RunPigeonsTest(Hrib id, string shardPath)
    {
        string arguments = GetPigeonsTestCommand(id, shardPath, GetHomeworkType());
        BlenderProcessOutput output = await Blender.RunBlenderCommand(arguments, pigeonsTestTimeoutMs);
        if (!output.Success)
        {
            return BlendInfo.Invalid(output.Message);
        }

        FileInfo shardFile = new FileInfo(shardPath);
        if (!shardFile.Exists || shardFile.DirectoryName is null)
        {
            return BlendInfo.Invalid("Shard file was not found");
        }
        DirectoryInfo shardDir = new DirectoryInfo(shardFile.DirectoryName);
        if (shardDir is null)
        {
            return BlendInfo.Invalid("Shard file was not found");
        }
        string testResultPath = shardDir.GetFiles()
            .Where(f => f.Name.StartsWith("pigeons_test_result") && f.Name.EndsWith(".json"))
            .OrderByDescending(f => f.LastWriteTime)
            .First()
            .ToString();

        string jsonContent = File.ReadAllText(testResultPath);
        List<BlendTestInfo> tests = new List<BlendTestInfo>();
        PigeonsTestResult? pigeonsResult = System.Text.Json.JsonSerializer.Deserialize<PigeonsTestResult>(jsonContent);
        if (pigeonsResult != null)
        {
            foreach (var result in pigeonsResult.Values)
            {
                tests.Add(new BlendTestInfo(
                    result.label,
                    result.state,
                    result.datablock,
                    result.message,
                    result.traceback
                ));
            }
        }
        return new BlendInfo(
            Const.BlendFileExtension,
            Const.BlendMimeType,
            tests.ToImmutableArray()
        );
    }
}