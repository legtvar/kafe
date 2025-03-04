using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe;

public readonly record struct KafeObject(
    KafeType Type,
    object Value
)
{
    public static readonly KafeObject Invalid = new(
        Type: KafeType.Invalid,
        Value: null!
    );

    public bool IsInvalid => Type == KafeType.Invalid || Value is null;

    public static bool TrySet(
        KafeObject? existing,
        KafeObject? @new,
        ExistingKafeObjectHandling existingValueHandling,
        out KafeObject? result,
        [NotNullWhen(false)] out LocalizedString? error
    )
    {
        result = null;
        error = null;

        switch (existingValueHandling)
        {
            case ExistingKafeObjectHandling.OverwriteExisting:
                result = @new;
                return true;

            case ExistingKafeObjectHandling.KeepExisting:
                result = existing is null || existing.Value.IsInvalid ? @new : existing;
                return true;

            case ExistingKafeObjectHandling.Merge:
                if (existing is null)
                {
                    result = @new;
                    return true;
                }

                if (@new is null)
                {
                    result = existing;
                    return true;
                }

                if (existing.Value.Type.IsArray || @new.Value.Type.IsArray)
                {
                    return TryMergeArrays(existing.Value, @new.Value, out result, out error);
                }
                
                if (existing.Value.Type != @new.Value.Type) {
                    result = existing;
                    error = LocalizedString.CreateInvariant(
                        $"Cannot merge a '{existing.Value.Type}' with a '{@new.Value.Type}' due to a type mismatch.");
                    return false;
                }

                if (existing.Value.Value is IMergeable mergeable)
                {
                    if (mergeable.TryMergeWith(@new.Value, out var merged))
                    {
                        return new KafeObject(
                            Type: existing.Value.Type,
                            Value: merged
                        );
                    }
                    else
                    {
                        result = existing;
                        error = LocalizedString.CreateInvariant(
                            ""
                        )
                    }
                }
        }
    }

    private static bool TryMergeArrays(
        KafeObject existing,
        KafeObject @new,
        out KafeObject? result,
        [NotNullWhen(false)] out LocalizedString? error
    )
    {
        result = existing;
        error = null;

        if (existing.Type.IsArray && !@new.Type.IsArray
            && existing.Type.GetElementType() == @new.Type)
        {
            result = new KafeObject(
                Type: existing.Type,
                Value: ((ImmutableArray<object>)existing.Value).Add(@new.Value)
            );
            return true;
        }

        if (!existing.Type.IsArray && @new.Type.IsArray
            && @new.Type.GetElementType() == existing.Type)
        {
            result = new KafeObject(
                Type: existing.Type,
                Value: ImmutableArray.Create(existing.Value).AddRange((ImmutableArray<object>)@new.Value)
            );
            return true;
        }

        if (existing.Type.IsArray && @new.Type.IsArray
            && existing.Type.GetElementType() == @new.Type.GetElementType())
        {
            result = new KafeObject(
                Type: existing.Type.GetElementType(),
                Value: ((ImmutableArray<object>)existing.Value)
                    .AddRange((ImmutableArray<object>)@new.Value)
            );
            return true;
        }

        error = LocalizedString.CreateInvariant($"Cannot merge a '{existing.Type}' with a '{@new.Type}' into an array. "
            + "The array's element type would not match.");
        return false;
    }
}

public class KafeObjectJsonConverter : JsonConverter<KafeObject>
{
    private readonly KafeTypeRegistry typeRegistry;

    public KafeObjectJsonConverter(KafeTypeRegistry typeRegistry)
    {
        this.typeRegistry = typeRegistry;
    }

    public override KafeObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, KafeObject value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
