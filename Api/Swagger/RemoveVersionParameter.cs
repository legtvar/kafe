using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using Microsoft.OpenApi;

namespace Kafe.Api.Swagger;

public class RemoveVersionParameter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var versionParameter = operation.Parameters?.SingleOrDefault(p => p.Name == "version");
        if (versionParameter is not null)
        {
            operation.Parameters?.Remove(versionParameter);
        }
    }

}
