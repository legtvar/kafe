using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kafe.Media;
using Kafe.Media.Services;
using Xunit;

namespace Kafe.Tests;

public sealed class VideoConversionTests
{
    [Theory]
    [InlineData(1280, 720, "sd,hd")]
    [InlineData(1920, 1080, "sd,hd,fullhd")]
    public async Task VideoConversion_SupportedSourceResolutions_CreatesAllApplicableVariants(
        int width,
        int height,
        string expectedPresetNames)
    {
        var mediaService = new FFmpegCoreService();
        var directory = Path.Combine(Path.GetTempPath(), $"kafe-video-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var sourcePath = Path.Combine(directory, "source.mp4");
            var outputDirectory = Path.Combine(directory, "generated");
            Directory.CreateDirectory(outputDirectory);
            await CreateVideoAsync(sourcePath, width, height);

            var source = await mediaService.GetInfo(sourcePath, TestContext.Current.CancellationToken);
            Assert.False(source.IsCorrupted, source.Error);
            var sourceVideo = Assert.Single(source.VideoStreams);
            Assert.Equal(width, sourceVideo.Width);
            Assert.Equal(height, sourceVideo.Height);

            var expectedPresets = expectedPresetNames.Split(',')
                .Select(Video.GetPresetFromFileName)
                .ToArray();
            var applicablePresets = Video.GetApplicablePresets(source);
            Assert.Equal(expectedPresets, applicablePresets);

            foreach (var preset in applicablePresets)
            {
                var variant = await mediaService.CreateVariant(
                    sourcePath,
                    preset,
                    outputDirectory,
                    overwrite: true,
                    token: TestContext.Current.CancellationToken);

                Assert.False(variant.IsCorrupted, variant.Error);
                var outputPath = Path.Combine(outputDirectory, $"{preset.ToFileName()}.webm");
                Assert.True(File.Exists(outputPath), $"Variant '{preset}' was not created at '{outputPath}'.");
                Assert.True(new FileInfo(outputPath).Length > 0, $"Variant '{preset}' is empty.");
                var video = Assert.Single(variant.VideoStreams);
                Assert.Equal(preset.ToWidth(), video.Width);
                Assert.Equal(preset.ToHeight(), video.Height);
            }
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static async Task CreateVideoAsync(string path, int width, int height)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo("ffmpeg")
        {
            RedirectStandardError = true,
            UseShellExecute = false
        };
        var arguments = process.StartInfo.ArgumentList;
        arguments.Add("-hide_banner");
        arguments.Add("-loglevel");
        arguments.Add("error");
        arguments.Add("-nostdin");
        arguments.Add("-y");
        arguments.Add("-f");
        arguments.Add("lavfi");
        arguments.Add("-i");
        arguments.Add($"color=c=blue:s={width}x{height}:r=25");
        arguments.Add("-f");
        arguments.Add("lavfi");
        arguments.Add("-i");
        arguments.Add("sine=frequency=440:sample_rate=48000");
        arguments.Add("-t");
        arguments.Add("1");
        arguments.Add("-c:v");
        arguments.Add("libx264");
        arguments.Add("-preset");
        arguments.Add("ultrafast");
        arguments.Add("-crf");
        arguments.Add("30");
        arguments.Add("-pix_fmt");
        arguments.Add("yuv420p");
        arguments.Add("-c:a");
        arguments.Add("aac");
        arguments.Add("-shortest");
        arguments.Add(path);

        Assert.True(process.Start());
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        Assert.True(process.ExitCode == 0, $"FFmpeg failed to create the test video: {error}");
    }
}
