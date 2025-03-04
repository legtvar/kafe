using System;
using System.Collections.Immutable;
using System.Reflection;
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

    public static KafeObject? Set(
        KafeObject? existing,
        KafeObject? @new,
        ExistingKafeObjectHandling existingValueHandling,
        out LocalizedString? error
    )
    {
        error = null;

        switch (existingValueHandling)
        {
            case ExistingKafeObjectHandling.OverwriteExisting:
                return @new;

            case ExistingKafeObjectHandling.KeepExisting:
                return existing is null || existing.Value.IsInvalid ? @new : existing;

            case ExistingKafeObjectHandling.MergeOrOverwrite:
            case ExistingKafeObjectHandling.MergeOrKeep:
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
                        return existingValueHandling == ExistingKafeObjectHandling.MergeOrOverwrite ? @new : existing;
                    }
                    return result;
                }

                if (existing.Value.Value is IMergeable mergeable)
                {
                    var merged = mergeable.GetType().InvokeMember(
                        nameof(IMergeable.Merge),
                        BindingFlags.Static,
                        null,
                        null,
                        [existing.Value, @new.Value]
                    ) as KafeObject?;
                    if (merged is not null)
                    {
                        return new KafeObject(
                            Type: existing.Value.Type,
                            Value: merged.Value
                        );
                    }
                }

                error = LocalizedString.CreateInvariant(
                    $"Cannot merge '{existing.Value.Type}' with '{@new.Value.Type}'."
                );
                return existingValueHandling == ExistingKafeObjectHandling.MergeOrOverwrite ? @new : existing;
            default:
                throw new NotImplementedException(
                    $"{nameof(ExistingKafeObjectHandling)} '{existingValueHandling}' is not implemented.");
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
