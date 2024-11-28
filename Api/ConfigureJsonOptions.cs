using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Kafe.Api;

public class ConfigureJsonOptions : IConfigureOptions<JsonOptions>
{
    private readonly IHostEnvironment environment;

    public ConfigureJsonOptions(IHostEnvironment environment)
    {
        this.environment = environment;
    }

    public void Configure(JsonOptions o)
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        o.JsonSerializerOptions.Converters.Add(new LocalizedStringJsonConverter());
        o.JsonSerializerOptions.Converters.Add(new HribJsonConverter());
        o.JsonSerializerOptions.Converters.Add(new ErrorJsonConverter()
        {
            ShouldWriteStackTraces = environment.IsDevelopment() || environment.IsStaging()
        });
        o.JsonSerializerOptions.Converters.Add(new ImmutableArrayJsonConverter());
    }
}
