namespace Kafe;

public readonly partial record struct Error
{
    public const string NotFoundId = nameof(NotFound);
    public const string BadHribId = nameof(BadHrib);
    public const string MissingValueId = nameof(MissingValue);
    public const string UnmodifiedId = nameof(Unmodified);
    public const string LockedId = nameof(Locked);
    public const string InvalidValueId = nameof(InvalidValue);
    public const string InvalidParameterValueId = nameof(InvalidParameterValue);
    public const string AlreadyExistsId = nameof(AlreadyExists);
    public const string ParameterArgument = "parameter";
    public const string BadKafeTypeId = nameof(BadKafeType);
    public const string InvalidMimeTypeId = nameof(InvalidMimeType);
    public const string ValueArgument = "value";
    public const string ShardAnalysisFailureId = nameof(ShardAnalysisFailure);

    public static Error NotFound(string? message = null)
    {
        return new Error(NotFoundId, message ?? "Could not be found.");
    }

    public static Error NotFound(Hrib id, string description = "An entity")
    {
        return new Error(NotFoundId, $"{description} with identifier '{id}' could not be found.");
    }

    public static Error BadHrib(string? value = null)
    {
        value = string.IsNullOrEmpty(value) ? "null" : $"'{value}'";
        return new Error(BadHribId, $"String {value} is not a valid identifier.");
    }

    public static Error InvalidOrEmptyHrib(string? description = null)
    {
        return new Error(BadHribId, description is null
            ? $"Encounterd an invalid or empty identifier."
            : $"{description} must be a valid and non-empty identifier.");
    }

    public static Error MissingValue(string what)
    {
        return new Error(MissingValueId, $"Value for '{what}' is missing.");
    }

    public static Error Unmodified(string what)
    {
        return new Error(UnmodifiedId, $"No changes were made to '{what}'.");
    }

    public static Error Unmodified(Hrib id, string description = "An entity")
    {
        return new Error(UnmodifiedId, $"No changed were made to {description} with identifier '{id}'.");
    }

    public static Error Locked(Hrib id, string description = "The entity")
    {
        return new Error(LockedId, $"{description} is locked.");
    }

    public static Error InvalidParameterValue(string what, string? parameterArg)
    {
        var error = new Error(InvalidParameterValueId, $"{what} has an invalid value.");
        if (!string.IsNullOrEmpty(parameterArg))
        {
            error = error.WithArgument(ParameterArgument, parameterArg);
        }

        return error;
    }

    public static Error InvalidValue(string message)
    {
        return new Error(InvalidValueId, message);
    }

    public static Error AlreadyExists(string? message = null)
    {
        return new Error(AlreadyExistsId, message ?? "Value already exists.");
    }

    public static Error AlreadyExists(Hrib id, string description = "An entity")
    {
        return new Error(AlreadyExistsId, $"{description} with identifier '{id}' already exists.");
    }

    public static Error BadKafeType(string? value)
    {
        return new Error(BadKafeTypeId, string.IsNullOrEmpty(value)
            ? "Encountered an invalid or empty KAFE type."
            : $"String '{value}' could not be parsed as a KAFE type.");
    }

    public static Error InvalidMimeType(string value)
    {
        return new Error(
            id: InvalidMimeTypeId,
            message: $"String '{value}' is not recognized as any known MIME type."
        ).WithArgument(ValueArgument, value);
    }

    public static Error ShardAnalysisFailure(KafeType shardType)
    {
        return new Error(
            id: ShardAnalysisFailureId,
            message: $"The provided data cannot be analyzed as a shard of type '{shardType}'."
        ).WithArgument(nameof(shardType), shardType);
    }
}
