using System;
using System.Text.Json;
using Xunit;

namespace Kafe.Tests;

public class FloatToIntJsonConverterTests
{
    [Theory]
    [InlineData("0", 0)]
    [InlineData("680030.0", 680030)]
    [InlineData("-42.000", -42)]
    [InlineData("9223372036854775807.0", long.MaxValue)]
    public void Read_AcceptsIntegralNumbers(string json, long expected)
    {
        var actual = JsonSerializer.Deserialize<long?>(json, CreateOptions());
        Assert.Equal<long?>(expected, actual);
    }

    [Theory]
    [InlineData("1.5")]
    [InlineData("9223372036854775808")]
    [InlineData("\"680030\"")]
    public void Read_RejectsUnsupportedValues(string json)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<long?>(json, CreateOptions()));
    }

    [Fact]
    public void Write_UsesIntegerRepresentation()
    {
        Assert.Equal("680030", JsonSerializer.Serialize(680030L, CreateOptions()));
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new NewtonsoftCompatibleInt64JsonConverter());
        return options;
    }
}
