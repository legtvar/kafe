using System;
using System.Reflection;

namespace Kafe;

public static class ReflectionExtensions
{
    public static object? GetStaticPropertyValue(
        this Type type,
        string propertyName,
        bool isRequired = true,
        bool allowNull = true,
        BindingFlags additionalBindingFlags = BindingFlags.Public
    )
    {
        var property = type.GetProperty(propertyName, BindingFlags.Static | additionalBindingFlags);
        if (property is null)
        {
            if (isRequired)
            {
                throw new ArgumentException($"Type '{type}' is missing a static property named '{propertyName}'.");
            }

            return null;
        }

        var propertyValue = property.GetValue(null);
        if (!allowNull && propertyValue is null)
        {
            throw new ArgumentException($"The '{propertyName}' static property of type '{type}' must be null.");
        }

        return propertyValue;
    }

    public static TPropertyType? GetStaticPropertyValue<TPropertyType>(
        this Type type,
        string propertyName,
        bool isRequired = true,
        bool allowNull = true,
        BindingFlags additionalBindingFlags = BindingFlags.Public
    )
    {
        var propertyValue = GetStaticPropertyValue(type, propertyName, isRequired, allowNull, additionalBindingFlags);
        if (propertyValue is not TPropertyType typedPropertyValue)
        {
            throw new ArgumentException(
                $"The '{type}.{propertyName}' static property must be of type '{typeof(TPropertyType)}'.");
        }

        return typedPropertyValue;
    }
}
