using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Kafe.Api;

public class ValidationProblemDetailsWithStackTrace : ValidationProblemDetails, IWithStackTrace
{
    public ValidationProblemDetailsWithStackTrace(ValidationProblemDetails pd)
    {
        Type = pd.Type;
        Title = pd.Title;
        Status = pd.Status;
        Detail = pd.Detail;
        Instance = pd.Instance;
        Extensions = pd.Extensions;
        Errors = pd.Errors;
    }

    [JsonIgnore]
    public StackTrace? StackTrace { get; set; }
}
