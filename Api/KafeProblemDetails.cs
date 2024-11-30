using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kafe.Api;

public class KafeProblemDetails : ProblemDetails
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("errors")]
    public ImmutableArray<Error> Errors { get; set; } = [];

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
        
        if (httpContext is not null)
        {
            var apiBehaviorOptions = httpContext.RequestServices
                .GetService<IOptions<ApiBehaviorOptions>>();
            if (apiBehaviorOptions is not null
                && apiBehaviorOptions.Value.ClientErrorMapping.TryGetValue(statusCode.Value, out var clientErrorData))
            {
                pd.Title ??= clientErrorData.Title;
                pd.Type ??= clientErrorData.Link;
            }
        }

        var errors = ImmutableArray.CreateBuilder<Error>();
        if (modelState is not null)
        {
            foreach (var (parameter, entry) in modelState)
            {
                foreach (var validationError in entry.Errors)
                {
                    errors.Add(new Error(
                        id: Error.InvalidValueId,
                        message: validationError.ErrorMessage,
                        arguments: ImmutableDictionary.CreateRange(
                            [
                                new KeyValuePair<string, object>(
                                    Error.ParameterArgument,
                                    parameter)
                            ]),
                        skipFrames: skipFrames + 1 // NB: The plus one account for the ctor itself.
                    ));
                }
            }
        }

        pd.Errors = errors.ToImmutable();
        return pd;
    }
}
