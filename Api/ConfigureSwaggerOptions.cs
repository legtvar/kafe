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
    public void Configure(SwaggerGenOptions o)
    {
        o.SwaggerDoc("v1", new OpenApiInfo { Title = "KAFE API", Version = "v1" });
        o.SupportNonNullableReferenceTypes();
        o.DocumentFilter<KafeComponentFilter>();
        o.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
        o.SchemaFilter<OptionalErrorStackTraceSchemaFilter>();
        o.MapType<TimeSpan>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "time-span",
                Example = new OpenApiString("00:00:00")
            }
        );
        o.EnableAnnotations(
            enableAnnotationsForInheritance: true,
            enableAnnotationsForPolymorphism: true
        );
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
            }
        );
        o.UseOneOfForPolymorphism();
        var xmlFiles = new DirectoryInfo(AppContext.BaseDirectory).GetFiles("*.xml");
        foreach (var xmlFile in xmlFiles)
        {
            o.IncludeXmlComments(xmlFile.FullName);
        }
    }
}

public class KafeComponentFilter(IOptions<DataOptions> dataOptions) : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        void Register(Type type, OpenApiSchema schema)
        {
            swaggerDoc.Components.Schemas[type.Name] = schema;
            if (context.SchemaRepository.TryLookupByType(type, out var existingSchema))
            {
                if (existingSchema.Reference.Id != type.Name)
                {
                    throw new InvalidOperationException(
                        $"Cannot re-register schema for type '{type.FullName}'. "
                        + $"It is already mapped to schema '{existingSchema.Reference.Id}'."
                    );
                }
            }
            else
            {
                context.SchemaRepository.RegisterType(type, type.Name);
            }
        }

        var properties = new Dictionary<string, OpenApiSchema>()
        {
            [Const.InvariantCultureCode] = new OpenApiSchema() { Type = "string" },
        };

        foreach (var languageCode in dataOptions.Value.Languages.Except([Const.InvariantCultureCode]))
        {
            properties[languageCode] = new OpenApiSchema() { Type = "string", Nullable = true };
        }

        var lsSchema = new OpenApiSchema
        {
            Title = nameof(LocalizedString),
            Description = "Multilingual string",
            Type = "object",
            Properties = properties,
            Nullable = false,
        };
        Register(typeof(LocalizedString), lsSchema);

        var hribSchema = new OpenApiSchema
        {
            Title = nameof(Hrib),
            Description = "Human-Readable Identifier Ballast (HRIB)",
            Type = "string",
            Format = "hrib",
            Example = new OpenApiString("AAAAbadf00d")
        };
        Register(typeof(Hrib), hribSchema);

        ImmutableArray<IOpenApiAny> permissionEnumValues =
        [
            .. Enum.GetValues<Permission>()
                .Where(v => BitOperations.PopCount((uint)v) == 1)
                .Select(v =>
                    {
                        var name = v.ToString();
                        name = char.ToLowerInvariant(name[0]) + name[1..];
                        return new OpenApiString(name) as IOpenApiAny;
                    }
                )
        ];
        var permissionSchema = new OpenApiSchema()
        {
            Title = nameof(Permission),
            Type = "string",
            Enum = permissionEnumValues
        };
        Register(typeof(Permission), permissionSchema);
    }
}
