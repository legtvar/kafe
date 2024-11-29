using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Kafe.Api;

public class ProblemDetailsWithStackTrace : ProblemDetails, IWithStackTrace
{
    public ProblemDetailsWithStackTrace(ProblemDetails pd)
    {
        Type = pd.Type;
        Title = pd.Title;
        Status = pd.Status;
        Detail = pd.Detail;
        Instance = pd.Instance;
        Extensions = pd.Extensions;
    }

    [JsonIgnore]
    public StackTrace? StackTrace { get; set; }
}
