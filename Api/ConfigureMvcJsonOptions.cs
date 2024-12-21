using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Kafe.Api;

public class ConfigureMvcJsonOptions : IConfigureOptions<JsonOptions>
{
    private readonly IHostEnvironment environment;

    public ConfigureMvcJsonOptions(IHostEnvironment environment)
    {
        this.environment = environment;
    }

    public void Configure(JsonOptions o)
    {
        // NB: There are two versions of JsonOptions, one for the entirety of ASP.NET Core, one just for MVC
        //     (see https://github.com/dotnet/aspnetcore/issues/57891).
        //     Here the same options are applied to Mvc.JsonOptions as to Http.Json.JsonOptions.
        ConfigureHttpJsonOptions.ConfigureSerializerOptions(o.JsonSerializerOptions, environment);
    }
}
