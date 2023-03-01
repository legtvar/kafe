using System.Globalization;
using System.Text.Json;
using Kafe.Data;
using Xunit;

namespace Kafe.Tests;

public class LocalizedStringTests
{
    [Fact]
    public void Test1()
    {
        var sample = LocalizedString.Create("Hello, World!", CultureInfo.CreateSpecificCulture("cs"), "Ahoj, Světe!");
        var options = new JsonSerializerOptions();
        options.Converters.Add(new LocalizedStringJsonConverter());
        var serializedSample = JsonSerializer.Serialize(sample, options);
        var deserializedSample = JsonSerializer.Deserialize<LocalizedString>(serializedSample, options);
        Assert.Equal(sample, deserializedSample);

        var sample2 = LocalizedString.Create(
            (CultureInfo.CreateSpecificCulture("cs"), "Nashle, Světe!"),
            (CultureInfo.InvariantCulture, "Bye, World!"));
        Assert.NotEqual(sample, sample2);
        Assert.NotEqual(deserializedSample, sample2);
    }
}
