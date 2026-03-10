using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Kafe.Api.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kafe.Api;

public class KafeProblemDetails : ProblemDetails
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("diagnostics")]
    public ImmutableArray<Diagnostic> Diagnostics { get; set; } = [];

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }

    public static KafeProblemDetails Create(
        HttpContext? httpContext = null,
        ModelStateDictionary? modelState = null,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null,
        int skipFrames = 1 // the 1 account for the frame of this Create method
    )
    {
        statusCode ??= 400;

        var pd = new KafeProblemDetails
        {
            Type = type,
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = instance,
            TraceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier
        };

        var apiBehaviorOptions = httpContext?.RequestServices.GetService<IOptions<ApiBehaviorOptions>>();
        if (apiBehaviorOptions is not null
            && apiBehaviorOptions.Value.ClientErrorMapping.TryGetValue(statusCode.Value, out var clientErrorData)
        )
        {
            pd.Title ??= clientErrorData.Title;
            pd.Type ??= clientErrorData.Link;
        }

        var errors = ImmutableArray.CreateBuilder<Diagnostic>();
        if (modelState is not null)
        {
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
                        ).ForParameter(parameter)
                    );
                }
                errors.Add(Diagnostic.Aggregate(entryErrors.ToImmutable()).ForParameter(parameter));
            }
        }

        pd.Diagnostics = errors.ToImmutable();
        return pd;
    }

    public IActionResult ToActionResult()
    {
        var objectResult = new ObjectResult(this)
        {
            StatusCode = Status
        };
        objectResult.ContentTypes.Add("application/problem+json");

        return objectResult;
    }
}
