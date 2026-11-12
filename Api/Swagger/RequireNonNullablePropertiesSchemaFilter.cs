using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using Microsoft.OpenApi;

namespace Kafe.Api.Swagger;

// based on: https://stackoverflow.com/questions/46576234/swashbuckle-make-non-nullable-properties-required
public class RequireNonNullablePropertiesSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// Add to model.Required all properties where Nullable is false.
    /// </summary>
    public void Apply(IOpenApiSchema model, SchemaFilterContext context)
    {
        var additionalRequiredProps = model.Properties?
            .Where(x => x.Value.Type?.HasFlag(JsonSchemaType.Null) == false && model.Required?.Contains(x.Key) == false)
            .Select(x => x.Key);
        foreach (var propKey in additionalRequiredProps ?? [])
        {
            model.Required?.Add(propKey);
        }
    }
}
