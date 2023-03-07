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
            UnauthorizedAccessException => 403,
            ArgumentOutOfRangeException => 404,
            IndexOutOfRangeException => 404,
            ArgumentException => 400,
            NotImplementedException => 501,
            NotSupportedException => 500,
            InvalidOperationException => 500,
            KafeException => 418,
            _ => 500
        };

        context.Result = environment.IsDevelopment()
            ? new ContentResult() {
                StatusCode = statusCode,
                ContentType = "text/plain",
                Content = context.Exception.ToString()
            }
            : new StatusCodeResult(statusCode);

        context.ExceptionHandled = true;
    }
}
