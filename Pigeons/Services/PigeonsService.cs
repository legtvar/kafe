using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using Kafe.Pigeons;

namespace Kafe.Pigeons.Services;

public class PigeonsService
{
    private static readonly ImmutableArray<string> supportedHomeworkTypes =
        ImmutableArray.Create(
            "Homework 2 - Composition",
            "Homework 3 - Chair",
            "Homework 3 - Your own model",
            "Homework 4 - Retopology",
            "Homework 5 - Materials"
        );

    private const int pigeonsTestTimeoutMs = 100000;

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

    public static string GetPigeonsTestOutputPath(string id, string filePath)
    {
        string? filePathDirectory = Path.GetDirectoryName(filePath);
        if (filePathDirectory is null)
        {
            throw new InvalidOperationException("Failed to find pigeons output file path directory .");
        }
        string hashSuffix = id + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        return Path.Combine(filePathDirectory, $"{Const.PigeonsTestOutputName}_{hashSuffix}.{Const.PigeonsTestOutputExtension}");
    }

    public string? GetHomeworkType(string projectGroupName)
    {
        if (!supportedHomeworkTypes.Contains(projectGroupName))
        {
            return null;
        }
        return projectGroupName.Replace(" ", "").Replace("-", "").ToLowerInvariant();
    }


    public static string GetPigeonsTestCommand(string id, string filePath, string type)
    {
        string args = string.Join(" ", new[]
        {
            "--background",
            "--python-exit-code", "1",
            "--python",$"\"{FindPigeonsManagerPath()}\"",
            "--",
            $"--hw={type}",
            $"--homework-file=\"{filePath}\"",
            "--output-to-file", $"\"{GetPigeonsTestOutputPath(id, filePath)}\""
        });
        return args;
    }

    public static string GetPigeonsUpdateCommand(bool allowOnline)
    {
        string args = string.Join(" ", new[]
        {
            "--background",
            "--python",$"\"{FindPigeonsManagerPath()}\"",
            "--",
            "update",
        });

        if (allowOnline)
        {
            args += " --allow-online";
        }
        return args;
    }

    public async Task<BlendInfo> RunPigeonsTest(string id, string shardPath, string projectGroupName)
    {
        string? homeworkType = GetHomeworkType(projectGroupName);
        if (homeworkType is null)
        {
            return new BlendInfo(
                Const.BlendFileExtension,
                Const.BlendMimeType,
                null
            );
        }
        string arguments = GetPigeonsTestCommand(id, shardPath, homeworkType);
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
        List<PigeonsTestInfo> tests = new List<PigeonsTestInfo>();
        PigeonsTestResultsSerializable? pigeonsResult = System.Text.Json.JsonSerializer.Deserialize<PigeonsTestResultsSerializable>(jsonContent);
        if (pigeonsResult != null)
        {
            foreach (var result in pigeonsResult.Values)
            {
                tests.Add(new PigeonsTestInfo(
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
