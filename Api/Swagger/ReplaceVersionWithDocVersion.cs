using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Kafe.Api.Swagger;

public class ReplaceVersionWithDocVersion : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var newPaths = new OpenApiPaths();
        foreach(var oldPath in swaggerDoc.Paths)
        {
            newPaths.Add(oldPath.Key.Replace("v{version}", swaggerDoc.Info.Version), oldPath.Value);
        }
        swaggerDoc.Paths = newPaths;
    }
}
