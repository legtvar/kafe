using Kafe.Data.Services;
using Xunit;

namespace Kafe.Tests;

public class ShardServiceTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData(" \t", null)]
    [InlineData("../../video.mp4", "video.mp4")]
    [InlineData("..\\..\\video.mp4", "video.mp4")]
    [InlineData("  vid\0eo.mp4  ", "video.mp4")]
    [InlineData("license.pdf", "license.pdf")]
    public void ShardService_SanitizeUploadFileName_ReturnsSanitizedNullableName(
        string? input,
        string? expected)
    {
        Assert.Equal(expected, ShardService.SanitizeUploadFileName(input));
    }

    [Fact]
    public void ShardService_SanitizeUploadFileName_TruncatesFileName()
    {
        var input = new string('a', ShardService.UploadFileNameMaxLength + 1) + ".pdf";

        var result = ShardService.SanitizeUploadFileName(input);

        Assert.NotNull(result);
        Assert.Equal(ShardService.UploadFileNameMaxLength, result.Length);
        Assert.Equal(
            new string('a', ShardService.UploadFileNameMaxLength - ".pdf".Length) + ".pdf",
            result);
    }
}
