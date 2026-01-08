using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kafe;

public static class ReflectionExtensions
{
    extension(Type type)
    {
        public object? GetStaticPropertyValue(
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

        public TPropertyType? GetStaticPropertyValue<TPropertyType>(
            string propertyName,
            bool isRequired = true,
            bool allowNull = true,
            BindingFlags additionalBindingFlags = BindingFlags.Public
        )
        {
            var propertyValue = type.GetStaticPropertyValue(propertyName, isRequired, allowNull, additionalBindingFlags);
            if (propertyValue is not TPropertyType typedPropertyValue)
            {
                throw new ArgumentException(
                    $"The '{type}.{propertyName}' static property must be of type '{typeof(TPropertyType)}'.");
            }

            return typedPropertyValue;
        }

        public bool IsNumeric()
        {
            var code = Type.GetTypeCode(type);
            return code >= TypeCode.SByte && code <= TypeCode.Decimal;
        }

        public bool IsInteger()
        {
            var code = Type.GetTypeCode(type);
            return code >= TypeCode.SByte && code <= TypeCode.UInt64;
        }
    }
}
