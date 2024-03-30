using Xunit;

namespace Kafe.Tests;

public class HribTests
{
    [Fact]
    public void ConvertFromString_WithValidHrib_ShouldNotThrow()
    {
        Hrib hrib1 = (Hrib)"abcdefghijk";
        Hrib hrib2 = "abcdefghijk";
    }

    [Fact]
    public void ConvertToString_WithValidHrib_ShouldNotThrow()
    {
        string str = (string)Hrib.Create();
    }
}
