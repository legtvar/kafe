using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
using Kafe.Api.Diagnostics;
using Kafe.Api.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace Kafe.Api;

public class KafeProblemDetailsFactory(
    KafeTypeRegistry typeRegistry,
    IOptions<ApiBehaviorOptions> apiBehaviorOptions,
    IOptions<ApiOptions> apiOptions,
    IHttpContextAccessor httpContextAccessor
) : ProblemDetailsFactory
{
    public KafeProblemDetails CreateProblemDetails(Diagnostic diagnostic)
    {
        // TODO: handle localization using Content-Language, Accept-Language, and the user's preferred language
        var diagnosticMessage = diagnostic.ToDiagnosticMessage(typeRegistry, Const.InvariantCulture);
        var metadataUrl = $"/api/v1/metadata/diagnostic?type={WebUtility.UrlEncode(diagnosticMessage.Type.ToString())}";
        return new KafeProblemDetails
        {
            Type = new Uri(new Uri(apiOptions.Value.BaseUrl), metadataUrl).AbsoluteUri,
            Title = diagnosticMessage.Type.ToString(),
            Detail = diagnosticMessage.Message,
            Diagnostic = diagnosticMessage,
            TraceId = Activity.Current?.Id ?? httpContextAccessor.HttpContext?.TraceIdentifier,
            Status = diagnostic.Payload is AspProblemDiagnostic aspProblem
                ? aspProblem.ProblemStatusCode
                : 400 // TODO: set proper status codes that fit the primary diagnostic payload
        };
    }


    public override KafeProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null
    )
    {
        return CreateProblemDetails(CreateDiagnostic(statusCode ?? 500, title, type, detail, instance));
    }

    public override ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ModelStateDictionary modelStateDictionary,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null
    )
    {
        throw new NotSupportedException(
            $"{nameof(KafeProblemDetailsFactory)} does not support {nameof(ValidationProblemDetails)}. "
            + $"Use {nameof(ParameterDiagnostic)} instead."
        );
    }

    public Diagnostic CreateDiagnostic(
        ModelStateDictionary modelState,
        int skipFrames = 1
    )
    {
        var errors = ImmutableArray.CreateBuilder<Diagnostic>();
        // TODO: Handle nested errors with JSON pointers.
        foreach (var (parameter, entry) in modelState)
        {
            var entryErrors = ImmutableArray.CreateBuilder<Diagnostic>();
            foreach (var validationError in entry.Errors)
            {
                entryErrors.Add(
                    new Diagnostic(
                        new MvcModelDiagnostic(validationError.ErrorMessage),
                        stackTrace: validationError.Exception?.StackTrace,
                        skipFrames: skipFrames + 1 // NB: The plus one account for the ctor itself.
                    )
                );
            }

            errors.Add(Diagnostic.Aggregate(entryErrors.ToImmutable()).ForParameter(parameter));
        }

        return new Diagnostic(new BadRequestDiagnostic(errors.ToImmutable()), skipFrames: skipFrames + 1);
    }

    public Diagnostic CreateDiagnostic(
        int statusCode,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null,
        int skipFrames = 1 // the 1 accounts for the frame of this Create method
    )
    {
        if (apiBehaviorOptions.Value.ClientErrorMapping.TryGetValue(statusCode, out var clientErrorData))
        {
            title ??= clientErrorData.Title;
            type ??= clientErrorData.Link;
        }

        return new Diagnostic(
            new AspProblemDiagnostic(
                ProblemStatusCode: statusCode,
                ProblemTitle: title,
                ProblemType: type,
                ProblemDetail: detail,
                ProblemInstance: instance
            ),
            skipFrames: skipFrames + 1
        );
    }
}
