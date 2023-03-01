using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Kafe.Api.Swagger;

public class RemoveVersionParameter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var versionParameter = operation.Parameters.Single(p => p.Name == "version");
        operation.Parameters.Remove(versionParameter);
    }
}
