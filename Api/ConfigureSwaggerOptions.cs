using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using Asp.Versioning;
using Kafe.Api.Swagger;
using Kafe.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kafe.Api;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IOptions<DataOptions> dataOptions;

    public ConfigureSwaggerOptions(IOptions<DataOptions> dataOptions)
    {
        this.dataOptions = dataOptions;
    }

    public void Configure(SwaggerGenOptions o)
    {
        o.SwaggerDoc("v1", new OpenApiInfo { Title = "KAFE API", Version = "v1" });
        o.SupportNonNullableReferenceTypes();
        o.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
        o.SchemaFilter<OptionalErrorStackTraceSchemaFilter>();
        o.MapType<Hrib>(() => new OpenApiSchema
        {
            Type = "string",
            Format = "hrib",
            Example = new OpenApiString("AAAAbadf00d")
        });
        o.MapType<TimeSpan>(() => new OpenApiSchema
        {
            Type = "string",
            Format = "time-span",
            Example = new OpenApiString("00:00:00")
        });
        o.MapType<LocalizedString>(() =>
        {
            var properties = new Dictionary<string, OpenApiSchema>()
            {
                [Const.InvariantCultureCode] = new OpenApiSchema() { Type = "string" },
            };

            foreach (var languageCode in dataOptions.Value.Languages.Except([Const.InvariantCultureCode]))
            {
                properties[languageCode] = new OpenApiSchema() { Type = "string", Nullable = true };
            }

            return new OpenApiSchema
            {
                Title = "LocalizedString",
                Type = "object",
                Nullable = true,
                Properties = properties
            };
        });
        o.SchemaFilter<PermissionSchemaFilter>();
        o.EnableAnnotations(
            enableAnnotationsForInheritance: true,
            enableAnnotationsForPolymorphism: true);
        o.OperationFilter<RemoveVersionParameter>();
        o.DocumentFilter<ReplaceVersionWithDocVersion>();
        o.DocInclusionPredicate((version, desc) =>
        {
            if (!desc.TryGetMethodInfo(out var method))
            {
                return false;
            }

            var versions = method.DeclaringType!.GetCustomAttributes(true)
                .OfType<ApiVersionAttribute>()
                .SelectMany(attr => attr.Versions);

            var maps = method.GetCustomAttributes(true)
                .OfType<MapToApiVersionAttribute>()
                .SelectMany(attr => attr.Versions)
                .ToArray();

            return versions.Any(v => $"v{v}" == version) && (maps.Length == 0 || maps.Any(v => $"v{v}" == version));
        });
        o.UseOneOfForPolymorphism();
        var xmlFiles = new DirectoryInfo(AppContext.BaseDirectory).GetFiles("*.xml");
        foreach (var xmlFile in xmlFiles)
        {
            o.IncludeXmlComments(xmlFile.FullName);
        }
    }
}

public class PermissionSchemaFilter : ISchemaFilter
{
    private static readonly ImmutableArray<IOpenApiAny> PermissionEnumValues = [.. Enum.GetValues<Permission>()
        .Where(v => BitOperations.PopCount((uint)v) == 1)
        .Select(v =>
        {
            var name = v.ToString();
            name = char.ToLowerInvariant(name[0]) + name[1..];
            return new OpenApiString(name) as IOpenApiAny;
        })];

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(Permission))
        {
            schema.Type = "string";
            schema.Enum = PermissionEnumValues;
            return;
        }

        var readOnlyListInterface = context.Type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)
                && i.GenericTypeArguments[0] == typeof(Permission)
        );

        if (readOnlyListInterface != null)
        {
            schema.Type = "array";
            schema.Items = new OpenApiSchema
            {
                Type = "string",
                Enum = PermissionEnumValues
            };
        }
    }
}
