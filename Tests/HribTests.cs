using Xunit;

namespace Kafe.Tests;

public class HribTests
{
    [Fact]
    public void Convert_FromString_ShouldNotThrow()
    {
        Hrib hrib1 = (Hrib)"abcdefghijk";
        Hrib hrib2 = "abcdefghijk";
    }

    [Fact]
    public void Convert_ToString_ShouldNotThrow()
    {
        string str = (string)Hrib.Create();
    }
}
