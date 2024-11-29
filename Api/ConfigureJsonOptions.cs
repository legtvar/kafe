using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Kafe.Api;

public class ConfigureJsonOptions : IConfigureOptions<JsonOptions>
{
    private readonly IHostEnvironment environment;

    public ConfigureJsonOptions(IHostEnvironment environment)
    {
        this.environment = environment;
    }

    public void Configure(JsonOptions o)
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        o.JsonSerializerOptions.Converters.Add(new LocalizedStringJsonConverter());
        o.JsonSerializerOptions.Converters.Add(new HribJsonConverter());
        o.JsonSerializerOptions.Converters.Add(new ErrorJsonConverter()
        {
            ShouldWriteStackTraces = environment.IsDevelopment() || environment.IsStaging()
        });
        // o.JsonSerializerOptions.Converters.Add(new ImmutableArrayJsonConverter());
        o.JsonSerializerOptions.TypeInfoResolver =
            (o.JsonSerializerOptions.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver())
            .WithAddedModifier(InitializeImmutableArrayProperties);
    }

    private static void InitializeImmutableArrayProperties(JsonTypeInfo type)
    {
        if (type.Kind == JsonTypeInfoKind.None)
        {
            return;
        }

        var immutableArrayProps = type.Properties.Where(p => p.PropertyType.IsGenericType
            && p.PropertyType.GetGenericTypeDefinition() == typeof(ImmutableArray<>)
            && p.Get is not null
            && p.Set is not null)
            .ToImmutableArray();

        if (immutableArrayProps.IsEmpty)
        {
            return;
        }


        bool IsNullOrDefaultImmutableArray(object? obj, Type arrayType)
        {
            if (obj is null)
            {
                return true;
            }

            var isDefaultProp = arrayType
                .GetProperty(
                    name: nameof(ImmutableArray<int>.IsDefault),
                    bindingAttr: BindingFlags.Public | BindingFlags.Instance)
                ?? throw new NotSupportedException("There is no IsDefault property on ImmutableArray.");

            return (bool)isDefaultProp.GetMethod!.Invoke(obj, null)!;
        }

        object CreateImmutableArray(Type type)
        {
            var genericCreate = typeof(ImmutableArray).GetMethod(
                name: nameof(ImmutableArray.Create),
                bindingAttr: BindingFlags.Static | BindingFlags.Public,
                types: Type.EmptyTypes)
                    ?? throw new NotSupportedException("There is no static parameterless Create method "
                        + "on ImmutableArray.");
            var create = genericCreate.MakeGenericMethod([type]);
            return create.Invoke(null, null)
                ?? throw new NotSupportedException($"An instance of ImmutableArray of '{type.Name}' "
                    + "could not be created.");
        }

        type.OnDeserialized = o =>
        {
            foreach (var immutableArrayProp in immutableArrayProps)
            {
                var array = immutableArrayProp.Get!(o);
                var elementType = immutableArrayProp.PropertyType.GenericTypeArguments.Single();
                if (IsNullOrDefaultImmutableArray(array, immutableArrayProp.PropertyType))
                {
                    immutableArrayProp.Set!(o, CreateImmutableArray(elementType));
                }
            }
        };
    }
}
