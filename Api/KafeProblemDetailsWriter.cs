using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Kafe.Api;

public class KafeProblemDetailsWriter : IProblemDetailsWriter
{
    private static readonly MediaTypeHeaderValue JsonMediaType = new("application/json");
    private static readonly MediaTypeHeaderValue ProblemDetailsJsonMediaType = new("application/problem+json");
    private readonly JsonSerializerOptions serializerOptions;

    public KafeProblemDetailsWriter(IOptions<JsonOptions> jsonOptions)
    {
        serializerOptions = jsonOptions.Value.SerializerOptions;
    }

    public bool CanWrite(ProblemDetailsContext context)
    {
        var acceptHeader = context.HttpContext.Request.Headers.Accept;
        if (acceptHeader.Count == 0)
        {
            // No accept header => send anything
            return true;
        }
        for (var i = 0; i < acceptHeader.Count; ++i)
        {
            var current = new MediaTypeHeaderValue(acceptHeader[i]!);
            if (current.IsSubsetOf(JsonMediaType)
                || current.IsSubsetOf(ProblemDetailsJsonMediaType)
                || JsonMediaType.IsSubsetOf(current))
            {
                return true;
            }
        }

        return false;
    }

    public ValueTask WriteAsync(ProblemDetailsContext context)
    {
        return new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(
            value: context.ProblemDetails,
            jsonTypeInfo: serializerOptions.GetTypeInfo(context.ProblemDetails.GetType()),
            contentType: ProblemDetailsJsonMediaType.ToString()));
    }
}
