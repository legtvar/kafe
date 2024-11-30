using Kafe.Api.Endpoints;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Kafe.Api;

public class ConfigureMvcOptions : IConfigureOptions<MvcOptions>
{
    public void Configure(MvcOptions o)
    {
        o.Conventions.Add(new RoutePrefixConvention(new RouteAttribute("/api/v{version:apiVersion}")));
        o.OutputFormatters.Insert(0, new HribOutputFormatter());
    }
}
