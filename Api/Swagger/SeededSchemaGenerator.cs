using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kafe.Api.Swagger;

public class SeededSchemaGenerator(
    SchemaGeneratorOptions options,
    SeededSchemaOptions seedOptions,
    ISerializerDataContractResolver dataContractResolver
) : ISchemaGenerator
{
    private readonly ISchemaGenerator inner = new SchemaGenerator(options, dataContractResolver);
    private bool isSeeded = false;

    public IOpenApiSchema GenerateSchema(
        Type modelType,
        SchemaRepository schemaRepository,
        MemberInfo? memberInfo = null,
        ParameterInfo? parameterInfo = null,
        ApiParameterRouteInfo? routeInfo = null
    )
    {
        if (!isSeeded)
        {
            foreach (var (type, schema) in seedOptions.Seeds)
            {
                RegisterSeed(schemaRepository, type, schema);
            }
            isSeeded = true;
        }

        return inner.GenerateSchema(modelType, schemaRepository, memberInfo, parameterInfo, routeInfo);
    }

    private void RegisterSeed(SchemaRepository schemaRepository, Type type, IOpenApiSchema schema)
    {
        schemaRepository.Schemas[type.Name] = schema;
        if (schemaRepository.TryLookupByType(type, out var existingSchema))
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
            schemaRepository.RegisterType(type, type.Name);
        }
    }
}
