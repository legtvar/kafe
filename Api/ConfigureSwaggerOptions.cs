using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json.Nodes;
using Asp.Versioning;
using Kafe.Api.Swagger;
using Kafe.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kafe.Api;

public class ConfigureSwaggerOptions(
    IOptions<DataOptions> dataOptions,
    SeededSchemaOptions seedOptions
    ) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions o)
    {
        AddSeedSchemas();

        o.SwaggerDoc("v1", new OpenApiInfo { Title = "KAFE API", Version = "v1" });
        o.SupportNonNullableReferenceTypes();
        o.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
        o.SchemaFilter<OptionalErrorStackTraceSchemaFilter>();
        o.MapType<LocalizedString>(() => new OpenApiSchemaReference(nameof(LocalizedString)));
        o.MapType<Hrib>(() => new OpenApiSchema
        {
            Description = "Human-Readable Identifier Ballast (HRIB)",
            Type = JsonSchemaType.String,
            Format = "hrib",
            Example = JsonValue.Create("AAAAbadf00d")
        });
        o.MapType<TimeSpan>(() => new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "time-span",
                Example = "00:00:00"
            }
        );
        o.EnableAnnotations(
            enableAnnotationsForInheritance: true,
            enableAnnotationsForPolymorphism: true
        );
        o.OperationFilter<RemoveVersionParameterFilter>();
        o.DocumentFilter<RemoveEndpointTagsFilter>();
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
            }
        );
        o.UseOneOfForPolymorphism();
        var xmlFiles = new DirectoryInfo(AppContext.BaseDirectory).GetFiles("*.xml");
        foreach (var xmlFile in xmlFiles)
        {
            o.IncludeXmlComments(xmlFile.FullName);
        }
    }

    private void AddSeedSchemas()
    {
        var properties = new Dictionary<string, IOpenApiSchema>()
        {
            [Const.InvariantCultureCode] = new OpenApiSchema() { Type = JsonSchemaType.String },
        };

        foreach (var languageCode in dataOptions.Value.Languages.Except([Const.InvariantCultureCode]))
        {
            properties[languageCode] = new OpenApiSchema() { Type = JsonSchemaType.String | JsonSchemaType.Null };
        }

        var lsSchema = new OpenApiSchema
        {
            Title = nameof(LocalizedString),
            Description = "Multilingual string",
            Type = JsonSchemaType.Object,
            Properties = properties,
        };
        seedOptions.Seeds[typeof(LocalizedString)] = lsSchema;

        ImmutableArray<JsonNode> permissionEnumValues =
        [
            .. Enum.GetValues<Permission>()
                .Where(v => BitOperations.PopCount((uint)v) == 1)
                .Select(v =>
                    {
                        var name = v.ToString();
                        name = char.ToLowerInvariant(name[0]) + name[1..];
                        return JsonValue.Create(name);
                    }
                )
        ];
        var permissionSchema = new OpenApiSchema()
        {
            Title = nameof(Permission),
            Type = JsonSchemaType.String,
            Enum = permissionEnumValues
        };
        seedOptions.Seeds[typeof(Permission)] = permissionSchema;

        var kafeTypeSchema = new OpenApiSchema()
        {
            Title = nameof(KafeType),
            Type = JsonSchemaType.String,
            Format = "kafe-type",
            Example = JsonValue.Create("core:scalar/string")
        };
        seedOptions.Seeds[typeof(KafeType)] = kafeTypeSchema;
    }
}
