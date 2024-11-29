# Binding `ImmutableArrays`

On my way to make API error types consistent, I bumped into an issue with the
[model binding](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding)
of `ImmutableArray` properties if they are completely missing in the request body's JSON.
In such a case an `InvalidOperationException` saying that
_"This operation cannot be performed on a default instance of ImmutableArray\<T\>."_ is thrown.

## Problem

The issue is fairly obvious: Since our DTOs are records, we don't explicitly initialize `ImmutableArray`s,
and thus we get _default_ arrays **not** _empty_ arrays. I had two options:

**1. Modify every DTO containing an `ImmutableArray` so that it initializes such members during construction.**

This would require a lot of tedious work and would result in less succint DTOs because we could no longer rely on primary constructors.

**2. Find a way to initialize the `ImmutableArray` properties during JSON deserialization.**

This would save me from the drawbacks of the 1. option, however, I had no idea how to do it.


## Wrong Solution: `ImmutableArrayJsonConverter`

At first, I thought a JSON converter would work. This is what I tried:

```csharp
public class ImmutableArrayJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        var gtd = type.GetGenericTypeDefinition();
        return gtd == typeof(ImmutableArray<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter)Activator.CreateInstance(
            typeof(InnerConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: null,
            culture: null)!;
    }

    private class InnerConverter<T> : JsonConverter<ImmutableArray<T>>
    {
        public override ImmutableArray<T> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var array = JsonSerializer.Deserialize<ImmutableArray<T>>(ref reader, options);
            if (array.IsDefault)
            {
                return [];
            }

            return array;
        }

        public override void Write(Utf8JsonWriter writer, ImmutableArray<T> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
```

This... didn't work. It didn't work because of the simple fact that the converter is only present for properties
that **are** present in the request's body, but those aren't the problematic ones!
It's the ones that aren't there that are the issue.

Also... for whatever reason the converter above causes a stack overflow.
I didn't bother investigating why since it didn't solve my problem in the first place.


## Correct-ish Solution: a `DefaultJsonTypeInfoResolver`'s modifier

Turns out that you can hook into the JSON deserializers process at the moment of model deserialization
(but before model binding) and do whatever you want to it.

First, this is how you register a modifier, provided you have a `Microsoft.AspNetCore.Mvc.JsonOptions` called `o`:

```csharp
o.JsonSerializerOptions.TypeInfoResolver =
    (o.JsonSerializerOptions.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver())
    .WithAddedModifier(InitializeImmutableArrayProperties);
```

Second, this is the modifier itself:

```csharp
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
```

If you think _"Boy, that's an awful, error-prone tangle of reflection"_, you're right.
But... it works... well enough.
The `Required` attribute no longer works on `ImmutableArray` properties though.
That's because, they're in truth initialized to the non-default value... :/

But... that's fine... for now.
It doesn't break anything I hope.
I might find a better solution someday.
It sure would be nice if I could modify the behavior of the `Required` attribute on these props somehow...
