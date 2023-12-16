using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using System;

namespace Kafe.Api;

public class SemanticExceptionFilter : IExceptionFilter
{
    private readonly IHostEnvironment environment;

    public SemanticExceptionFilter(IHostEnvironment environment)
    {
        this.environment = environment;
    }

    public void OnException(ExceptionContext context)
    {
        int statusCode = context.Exception switch
        {
            UnauthorizedAccessException => 403, // 403 Forbidden
            ArgumentOutOfRangeException => 404, // 404 Not Found
            IndexOutOfRangeException => 404, // 404 Not Found
            ArgumentException => 400, // 400 Bad Request
            NotImplementedException => 501, // 501 Not Implemented
            NotSupportedException => 500, // 500 Internal Server Error
            InvalidOperationException => 500, // 500 Internal Server Error
            KafeException => 418, // 418 I'm a teapot
            _ => 500 // 500 Internal Server Error
        };

        context.Result = environment.IsDevelopment()
            ? new ContentResult()
            {
                StatusCode = statusCode,
                ContentType = "text/plain",
                Content = context.Exception.ToString()
            }
            : new StatusCodeResult(statusCode);

        context.ExceptionHandled = true;
    }
}
