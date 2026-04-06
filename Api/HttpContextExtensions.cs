using System;
using Kafe.Api.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kafe.Api;

public static class HttpContextExtensions
{
    /// <summary>
    /// Returns a URL path fragment if the API uses the same hostname as the front end, or absolute URI otherwise.
    /// </summary>
    public static string GetFrontEndRedirect(this HttpContext context, string location)
    {
        var apiOptions = context.RequestServices.GetRequiredService<IOptions<ApiOptions>>();
        return apiOptions.Value.GetFrontEndRedirect(location);
    }
}
