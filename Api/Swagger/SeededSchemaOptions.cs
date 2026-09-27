using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Kafe.Api.Swagger;

public class SeededSchemaOptions
{
    public Dictionary<Type, OpenApiSchema> Seeds { get; } = [];
}
