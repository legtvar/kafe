using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace Kafe.Api;

public class ConfigureOpenApiOptions : IConfigureNamedOptions<OpenApiOptions>
{
    public void Configure(string? name, OpenApiOptions options)
    {
        options.AddDocumentTransformer((doc, ctx, ct) =>
        {
            doc.Info.Title = "KAFE API";
            doc.Info.Version = name;
            var newPaths = new OpenApiPaths();

            foreach (var oldPath in doc.Paths)
            {
                newPaths.Add(
                    oldPath.Key.Replace("v{version}", doc.Info.Version),
                    oldPath.Value);
            }

            doc.Paths = newPaths;
            return Task.CompletedTask;
        });

        options.AddOperationTransformer((op, ctx, ct) =>
        {
            var versionParam = op.Parameters.SingleOrDefault(p => p.Name == "version");
            if (versionParam is not null)
            {
                op.Parameters.Remove(versionParam);
            }
            return Task.CompletedTask;
        });
    }

    public void Configure(OpenApiOptions options)
    {
        throw new NotSupportedException($"{nameof(ConfigureOpenApiOptions)} can only be called on "
            + "named instances of OpenApiOptions.");
    }
}
