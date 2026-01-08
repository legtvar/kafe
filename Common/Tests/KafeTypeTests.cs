namespace Kafe.Common.Tests;

public class KafeTypeTests
{
    [Fact]
    public void KafeType_LocalizedString_HasMetadata()
    {
        Assert.Equal("localized-string", IKafeTypeMetadata.GetMoniker<LocalizedString>());
    }
}
