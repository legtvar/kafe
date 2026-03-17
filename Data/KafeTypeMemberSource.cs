using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JasperFx.Core.Reflection;
using Marten;
using Marten.Linq.Members;

namespace Kafe.Data;

public class KafeTypeMemberSource : IMemberSource
{
    public bool TryResolve(
        IQueryableMember parent,
        StoreOptions options,
        MemberInfo memberInfo,
        Type memberType,
        [NotNullWhen(true)] out IQueryableMember? member
    )
    {
        if (memberType != typeof(KafeType))
        {
            member = null;
            return false;
        }

        member = new StringValueTypeMember<KafeType>(
            parent,
            options.Serializer().Casing,
            memberInfo,
            new ValueTypeInfo(
                outerType: typeof(KafeType),
                simpleType: typeof(string),
                valueProperty: typeof(KafeType).GetProperty(
                    nameof(KafeType.RawValue),
                    BindingFlags.Instance | BindingFlags.NonPublic
                ) ?? throw new InvalidOperationException(
                    $"Could not find the '{nameof(KafeType)}.{nameof(KafeType.RawValue)}'."
                ),
                builder: typeof(KafeTypeMemberSource).GetMethod(
                    nameof(BuildKafeType),
                    BindingFlags.Static | BindingFlags.NonPublic
                )!
            )
        );
        return true;
    }

    private static KafeType BuildKafeType(string value)
    {
        return KafeType.Parse(value);
    }
}
