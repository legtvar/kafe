using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Kafe.Api;

/// <summary>
/// A wrapper around a <see cref="Diagnostic"/> that adheres to RFC 7807 (Problem details for HTTP APIs).
/// </summary>
public class KafeProblemDetails : ProblemDetails
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("diagnostic")]
    public DiagnosticMessage Diagnostic { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }

    public ActionResult ToActionResult()
    {
        var objectResult = new ObjectResult(this)
        {
            StatusCode = Status
        };
        objectResult.ContentTypes.Add("application/problem+json");

        return objectResult;
    }
}
