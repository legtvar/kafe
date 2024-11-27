using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Kafe.Api;

public class KafeProblemDetails : ProblemDetails
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImmutableArray<Error> Errors { get; set; } = [];
}
