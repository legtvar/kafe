using Xunit;

namespace Kafe.Tests;

public class KafeTypeTests
{
    [Theory]
    [InlineData("core:string", "core", "string", null, false)]
    [InlineData("media:shard/video", "media", "shard", "video", false)]
    [InlineData("core:number[]", "core", "number", null, true)]
    [InlineData("media:localized-shard/image[]", "media", "localized-shard", "image", true)]
    public void Parse_WithValidType_ShouldSucceed(
        string s,
        string mod,
        string primary,
        string? secondary,
        bool isArray
    )
    {
        Assert.True(KafeType.TryParse(s, out var kafeType));
        Assert.Equal(mod, kafeType.Mod);
        Assert.Equal(primary, kafeType.Category);
        Assert.Equal(secondary, kafeType.Moniker);
        Assert.Equal(isArray, kafeType.IsArray);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("core")]
    [InlineData("shard/video")]
    [InlineData("core:shard[]/video")]
    [InlineData("core:shard/*")]
    [InlineData("$:shard/video")]
    public void Parse_WithInvalidType_ShouldFail(
        string? s
    )
    {
        Assert.False(KafeType.TryParse(s, out _));
    }
}
