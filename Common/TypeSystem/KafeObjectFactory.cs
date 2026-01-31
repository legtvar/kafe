using System;
using System.Collections.Immutable;
using System.Reflection;

namespace Kafe;

public class KafeObjectFactory
{
    private readonly KafeTypeRegistry typeRegistry;

    public KafeObjectFactory(KafeTypeRegistry typeRegistry)
    {
        this.typeRegistry = typeRegistry;
    }

    public KafeObject Wrap(Type type, object value)
    {
        var valueType = value.GetType();
        if (!valueType.IsAssignableTo(type))
        {
            throw new ArgumentException($"Cannot wrap the value in a KafeObject because the value's type, "
                + $"'{valueType.FullName}', is not assignable to '{type.FullName}'.");
        }

        if (!typeRegistry.DotnetTypeMap.TryGetValue(type, out var kafeType))
        {
            throw new ArgumentException($"CLR type '{type.FullName}' has no registered KafeType.", nameof(type));
        }

        return new KafeObject(
            Type: kafeType,
            Value: value
        );
    }

    public KafeObject? Set(
        KafeObject? existing,
        KafeObject? @new,
        ExistingValueHandling existingValueHandling,
        out LocalizedString? error
    )
    {
        error = null;

        switch (existingValueHandling)
        {
            case ExistingValueHandling.OverwriteExisting:
                return @new;

            case ExistingValueHandling.KeepExisting:
                return existing is null || !existing.Value.IsValid ? @new : existing;

            case ExistingValueHandling.MergeOrOverwrite:
            case ExistingValueHandling.MergeOrKeep:
                if (existing is null)
                {
                    return @new;
                }

                if (@new is null)
                {
                    return existing;
                }

                if (existing.Value.Type.IsArray || @new.Value.Type.IsArray)
                {
                    var result = MergeArrays(existing.Value, @new.Value, out error);
                    if (result is null)
                    {
                        // error happened, fallback to either Keep or Overwrite
                        return existingValueHandling == ExistingValueHandling.MergeOrOverwrite ? @new : existing;
                    }
                    return result;
                }

                var mergableInterface = typeof(IMergeable<>).MakeGenericType(@new.GetType());
                if (existing.Value.Value.GetType().IsAssignableTo(mergableInterface))
                {
                    var merged = mergableInterface.InvokeMember(
                        nameof(IMergeable<byte>.MergeWith),
                        BindingFlags.Instance | BindingFlags.Public,
                        null,
                        existing.Value.Value,
                        [@new.Value]
                    );
                    if (merged is not null)
                    {
                        return this.Wrap(merged);
                    }
                }

                error = LocalizedString.CreateInvariant(
                    $"Cannot merge '{existing.Value.Type}' with '{@new.Value.Type}'."
                );
                return existingValueHandling == ExistingValueHandling.MergeOrOverwrite ? @new : existing;
            default:
                throw new NotImplementedException(
                    $"{nameof(ExistingValueHandling)} '{existingValueHandling}' is not implemented.");
        }
    }

    private static KafeObject? MergeArrays(
        KafeObject existing,
        KafeObject @new,
        out LocalizedString? error
    )
    {
        error = null;

        if (existing.Type.IsArray && !@new.Type.IsArray
            && existing.Type.GetElementType() == @new.Type)
        {
            return new KafeObject(
                Type: existing.Type,
                Value: ((ImmutableArray<object>)existing.Value).Add(@new.Value)
            );
        }

        if (!existing.Type.IsArray && @new.Type.IsArray
            && @new.Type.GetElementType() == existing.Type)
        {
            return new KafeObject(
                Type: existing.Type,
                Value: ImmutableArray.Create(existing.Value).AddRange((ImmutableArray<object>)@new.Value)
            );
        }

        if (existing.Type.IsArray && @new.Type.IsArray
            && existing.Type.GetElementType() == @new.Type.GetElementType())
        {
            return new KafeObject(
                Type: existing.Type.GetElementType(),
                Value: ((ImmutableArray<object>)existing.Value)
                    .AddRange((ImmutableArray<object>)@new.Value)
            );
        }

        error = LocalizedString.CreateInvariant($"Cannot merge a '{existing.Type}' with a '{@new.Type}' into an array. "
            + "The array's element type would not match.");
        return null;
    }
}
