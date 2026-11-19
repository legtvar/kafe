using System;
using System.Collections.Generic;
using Microsoft.OpenApi;

namespace Kafe.Api.Swagger;

public class SeededSchemaOptions
{
    public Dictionary<Type, IOpenApiSchema> Seeds { get; } = [];
}
