using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Kafe.Api;

public class UnsupportedProblemDetailsFactory : ProblemDetailsFactory
{

    public override ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        throw new NotSupportedException("KAFE does not support ProblemDetailsFactory. "
            + "Use KafeProblemDetails.Create instead.");
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
        throw new NotSupportedException("KAFE does not support ProblemDetailsFactory. "
            + "Use KafeProblemDetails.Create instead.");
    }
}
