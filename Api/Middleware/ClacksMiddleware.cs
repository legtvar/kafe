using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Kafe.Api.Middleware;

public class ClacksMiddleware : IMiddleware
{
    public const string HeaderName = "X-Clacks-Overhead";
    public const string HeaderValue = "GNU Terry Pratchett";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.TryAdd(HeaderName, HeaderValue);
            return Task.CompletedTask;
        });

        await next(context);
    }
}
