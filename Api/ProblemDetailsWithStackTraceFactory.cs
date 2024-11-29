using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace Kafe.Api;

public class ProblemDetailsWithStackTraceFactory : ProblemDetailsFactory
{
    private readonly DefaultProblemDetailsFactory inner;

    public ProblemDetailsWithStackTraceFactory(
        IOptions<ApiBehaviorOptions> options,
        IOptions<ProblemDetailsOptions>? problemDetailsOptions = null
    )
    {
        this.inner = new DefaultProblemDetailsFactory(options, problemDetailsOptions);
    }

    public override ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        var pd = inner.CreateProblemDetails(httpContext, statusCode, title, type, detail, instance);
        return new ProblemDetailsWithStackTrace(pd)
        {
            StackTrace = new StackTrace(skipFrames: 1, fNeedFileInfo: true)
        };
    }

    public override ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ModelStateDictionary modelStateDictionary,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        var pd = inner.CreateValidationProblemDetails(
            httpContext,
            modelStateDictionary,
            statusCode,
            title,
            type,
            detail,
            instance);
        return new ValidationProblemDetailsWithStackTrace(pd)
        {
            StackTrace = new StackTrace(skipFrames: 1, fNeedFileInfo: true)
        };
    }
}
