using System.Collections.Immutable;
using System.Linq;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kafe.Api.Swagger;

public class RemoveEndpointTagsFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (swaggerDoc.Tags is null)
        {
            return;
        }

        var endpointTags = swaggerDoc.Tags.Where(t => t.Name is not null && t.Name.EndsWith("Endpoint"))
            .ToImmutableArray();
        foreach (var tag in endpointTags)
        {
            swaggerDoc.Tags.Remove(tag);
        }
    }
}
