using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kafe.Api.Swagger;

public class OptionalErrorStackTraceSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(Diagnostic))
        {
            return;
        }
        
        schema.Required.Remove("stackTrace");
    }
}
