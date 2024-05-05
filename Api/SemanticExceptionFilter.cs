using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Kafe.Api;

public class SemanticExceptionFilter : IExceptionFilter
{
    private readonly IHostEnvironment environment;
    private readonly ILogger<SemanticExceptionFilter> logger;

    public SemanticExceptionFilter(IHostEnvironment environment, ILogger<SemanticExceptionFilter> logger)
    {
        this.environment = environment;
        this.logger = logger;
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

        context.Result = new ObjectResult(
            new ProblemDetails
            {
                Type = context.Exception.GetType().Name,
                Title = "An exception was thrown while processing your request.",
                Status = statusCode,
                Detail = context.Exception.ToString()
            })
        {
            StatusCode = statusCode,
        };

        context.ExceptionHandled = true;
        
        logger.LogError(
            context.Exception,
            "An exception occured while processing {ActionDisplayName}.",
            context.ActionDescriptor.DisplayName);
    }
}
