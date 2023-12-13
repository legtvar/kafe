using Xunit;

namespace Kafe.Tests;

public class HribTests
{
    [Fact]
    public void CastFromStringTest()
    {
        Hrib hrib1 = (Hrib)"abcdefghijk";
        Hrib hrib2 = "abcdefghijk";
    }
    
        [Fact]
    public void CastToStringTest()
    {
        string str = (string)Hrib.Create();
    }
}
