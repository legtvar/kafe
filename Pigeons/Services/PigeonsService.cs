using System;
using System.Runtime.InteropServices;
using System.Collections.Immutable;
using System.Text.Json;
using Kafe.Data.Options;
using Kafe.Mate;
using Microsoft.Extensions.Options;

namespace Kafe.Pigeons.Services;

public class PigeonsService(
    ILogger<PigeonsService> logger,
    IOptions<StorageOptions> options,
    IFindShardFile findShardFile
)
{
    private static readonly ImmutableArray<string> SupportedHomeworkTypes =
    [
        "Homework 2 - Composition",
        "Homework 3 - Chair",
        "Homework 3 - Your own model",
        "Homework 4 - Retopology",
        "Homework 5 - Materials"
    ];

    private const int PigeonsTestTimeoutMs = 100_000;

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
            throw new InvalidOperationException("Failed to find pigeons output file path directory.");
        }
        string hashSuffix = id + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        return Path.Combine(filePathDirectory, $"{Const.PigeonsTestOutputName}_{hashSuffix}.{Const.PigeonsTestOutputExtension}");
    }

    public string? GetHomeworkType(string projectGroupName)
    {
        if (!SupportedHomeworkTypes.Contains(projectGroupName))
        {
            return null;
        }
        return projectGroupName.Replace(" ", "").Replace("-", "").ToLowerInvariant();
    }


    public static string GetPigeonsTestCommand(string filePath, string outputPath, string type)
    {
        string args = string.Join(" ", new[]
        {
            "--background",
            "--python-exit-code", "1",
            "--python",$"\"{FindPigeonsManagerPath()}\"",
            "--",
            $"--hw={type}",
            $"--homework-file=\"{filePath}\"",
            "--output-to-file", $"\"{outputPath}\""
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

    public async Task<PigeonsTestResponse> RunPigeonsTest(PigeonsTestRequest request, CancellationToken ct = default)
    {
        var shardPathErr = await findShardFile.Find(request.ShardUri, ct);
        if (shardPathErr.HasError)
        {
            return new PigeonsTestResponse(
                Tests: [],
                Error: shardPathErr.Diagnostic.ToString(Kafe.Const.InvariantCulture)
            );
        }

        var shardPath = shardPathErr.Value;
        var homeworkType = GetHomeworkType(request.HomeworkType);
        if (homeworkType is null)
        {
            return new PigeonsTestResponse(
                Tests: [],
                Error: "No valid homework type found for the provided project group."
            );
        }

        var shardFile = new FileInfo(shardPath);
        if (!shardFile.Exists || shardFile.DirectoryName is null)
        {
            return new PigeonsTestResponse(
                Tests: [],
                Error: "Shard file was not found."
            );
        }

        var shardDir = new DirectoryInfo(shardFile.DirectoryName);
        if (!shardDir.Exists)
        {
            return new PigeonsTestResponse(
                Tests: [],
                Error: "Shard file directory was not found."
            );
        }

        var hash = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        var tmpName = Path.GetTempFileName();
        var outputDir = new DirectoryInfo(Path.Combine(options.Value.TempDirectory, tmpName));
        if (!outputDir.Exists)
        {
            outputDir.Create();
        }

        var outputPath = Path.Combine(
            outputDir.FullName,
            $"{Const.PigeonsTestOutputName}_{hash}.{Const.PigeonsTestOutputExtension}"
        );

        var arguments = GetPigeonsTestCommand(shardPath, outputPath, homeworkType);
        logger.LogInformation("Running Blender with the following arguments:\n\t{Arguments}", arguments);
        var output = await Blender.RunBlenderCommand(arguments, PigeonsTestTimeoutMs);
        if (!output.Success)
        {
            logger.LogInformation("Pigeons tests for shard '{ShardId}' failed.", shardDir.Name);
            return new PigeonsTestResponse(
                Tests: [],
                Error: output.Message
            );
        }

        var jsonContent = await File.ReadAllTextAsync(outputPath, ct);
        var tests = ImmutableArray.CreateBuilder<PigeonsTestInfo>();
        var pigeonsResult = JsonSerializer.Deserialize<Dictionary<int, PigeonsTestResultJson>>(jsonContent);
        if (pigeonsResult != null)
        {
            tests.AddRange(
                pigeonsResult.Values.Select(result => new PigeonsTestInfo(
                        result.Label,
                        result.State,
                        result.Datablock,
                        result.Message,
                        result.Traceback
                    )
                )
            );
        }
        return new PigeonsTestResponse(
            Tests: tests.ToImmutable(),
            Error: null
        );
    }
}
