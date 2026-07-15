using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Kafe.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Kafe.Api;

public class ConfigureJsonOptions(
    IHostEnvironment environment,
    KafeTypeRegistry typeRegistry
) : IConfigureOptions<JsonOptions>
{
    public void Configure(JsonOptions o)
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        o.JsonSerializerOptions.Converters.Add(new LocalizedStringJsonConverter());
        o.JsonSerializerOptions.Converters.Add(new HribJsonConverter());
        o.JsonSerializerOptions.Converters.Add(new KafeTypeJsonConverter(typeRegistry));
        o.JsonSerializerOptions.Converters.Add(
            new KafeObjectJsonConverterFactory(
                typeRegistry,
                (JsonConverter<KafeObject>)o.JsonSerializerOptions.GetConverter(typeof(KafeObject))
            )
        );
        o.JsonSerializerOptions.Converters.Add(new DiagnosticMessageJsonConverter(typeRegistry));
        o.JsonSerializerOptions.Converters.Add(new ReflectionTypeJsonConverter(typeRegistry));

        var typeInfoResolver = (o.JsonSerializerOptions.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver())
            .WithAddedModifier(InitializeImmutableArrayProperties)
            .WithAddedModifier(RemoveDotnetType);
        if (environment.IsProduction())
        {
            typeInfoResolver = typeInfoResolver.WithAddedModifier(RemoveErrorStackTraces);
        }

        o.JsonSerializerOptions.TypeInfoResolver = typeInfoResolver;
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
                && p.Set is not null
            )
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
                        bindingAttr: BindingFlags.Public | BindingFlags.Instance
                    )
                ?? throw new NotSupportedException("There is no IsDefault property on ImmutableArray.");

            var getMethod = isDefaultProp.GetMethod
                ?? throw new NotSupportedException("IsDefault property has no getter.");
            var result = getMethod.Invoke(obj, null)
                ?? throw new NotSupportedException("IsDefault invocation returned null.");
            return (bool)result;
        }

        object CreateImmutableArray(Type type)
        {
            var genericCreate = typeof(ImmutableArray).GetMethod(
                    name: nameof(ImmutableArray.Create),
                    bindingAttr: BindingFlags.Static | BindingFlags.Public,
                    types: Type.EmptyTypes
                )
                ?? throw new NotSupportedException(
                    "There is no static parameterless Create method "
                    + "on ImmutableArray."
                );
            var create = genericCreate.MakeGenericMethod([type]);
            return create.Invoke(null, null)
                ?? throw new NotSupportedException(
                    $"An instance of ImmutableArray of '{type.Name}' "
                    + "could not be created."
                );
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

    private static void RemoveErrorStackTraces(JsonTypeInfo type)
    {
        if (type.Type != typeof(Diagnostic) && type.Type != typeof(DiagnosticMessage)
            || type.Kind == JsonTypeInfoKind.None
        )
        {
            return;
        }

        var stackTraceProp = type.Properties
            .Single(p => (p.AttributeProvider as PropertyInfo)?.Name == nameof(DiagnosticMessage.StackTrace));
        type.Properties.Remove(stackTraceProp);
    }

    private static void RemoveDotnetType(JsonTypeInfo type)
    {
        if (type.Type != typeof(KafeObject))
        {
            return;
        }

        var dotnetTypeProp = type.Properties.Single(p => p.Name == nameof(KafeObject.DotnetType));
        type.Properties.Remove(dotnetTypeProp);
    }
}
