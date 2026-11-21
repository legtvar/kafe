using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Kafe.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Kafe.Tests;

public class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task UseSecurityHeaders_HttpsResponse_AddsConfiguredSecurityHeaders()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.Configure(app =>
                {
                    app.Use((context, next) =>
                    {
                        context.Request.Scheme = "https";
                        return next();
                    });
                    app.UseSecurityHeaders(ApiSecurityHeaders.Create());
                    app.Run(context =>
                    {
                        context.Response.StatusCode = 204;
                        return Task.CompletedTask;
                    });
                });
            })
            .StartAsync(TestContext.Current.CancellationToken);
        using var client = host.GetTestClient();
        client.BaseAddress = new Uri("https://kafe.fi.muni.cz");

        using var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        var expectedHeaders = new Dictionary<string, string>
        {
            ["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains",
            ["X-Frame-Options"] = "SAMEORIGIN",
            ["X-Content-Type-Options"] = "nosniff",
            ["Referrer-Policy"] = "strict-origin-when-cross-origin"
        };

        foreach (var (name, value) in expectedHeaders)
        {
            Assert.True(
                response.Headers.TryGetValues(name, out var values),
                $"Missing {name} for {response.RequestMessage?.RequestUri}. "
                    + string.Join("; ", response.Headers.Select(h => $"{h.Key}: {string.Join(",", h.Value)}")));
            Assert.Equal(value, values.Single());
        }

        var contentSecurityPolicy = response.Headers.GetValues("Content-Security-Policy").Single();
        Assert.Contains("default-src 'self'", contentSecurityPolicy);
        Assert.Contains("frame-ancestors 'self'", contentSecurityPolicy);
        Assert.Contains("script-src 'self' 'unsafe-inline'", contentSecurityPolicy);
        Assert.Contains("font-src 'self' data: https://fonts.gstatic.com", contentSecurityPolicy);
        Assert.Contains("upgrade-insecure-requests", contentSecurityPolicy);

        var permissionsPolicy = response.Headers.GetValues("Permissions-Policy").Single();
        Assert.Contains("camera=()", permissionsPolicy);
        Assert.Contains("microphone=()", permissionsPolicy);
        Assert.Contains("fullscreen=self", permissionsPolicy);
    }
}
