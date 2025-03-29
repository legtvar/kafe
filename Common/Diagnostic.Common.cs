namespace Kafe;

public static class Error
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

    public static Diagnostic NotFound(string? message = null)
    {
        return new Diagnostic(NotFoundId, message ?? "Could not be found.");
    }

    public static Diagnostic NotFound(Hrib id, string description = "An entity")
    {
        return new Diagnostic(NotFoundId, $"{description} with identifier '{id}' could not be found.");
    }

    public static Diagnostic BadHrib(string? value = null)
    {
        value = string.IsNullOrEmpty(value) ? "null" : $"'{value}'";
        return new Diagnostic(BadHribId, $"String {value} is not a valid identifier.");
    }

    public static Diagnostic InvalidOrEmptyHrib(string? description = null)
    {
        return new Diagnostic(BadHribId, description is null
            ? $"Encounterd an invalid or empty identifier."
            : $"{description} must be a valid and non-empty identifier.");
    }

    public static Diagnostic MissingValue(string what)
    {
        return new Diagnostic(MissingValueId, $"Value for '{what}' is missing.");
    }

    public static Diagnostic Unmodified(string what)
    {
        return new Diagnostic(UnmodifiedId, $"No changes were made to '{what}'.");
    }

    public static Diagnostic Unmodified(Hrib id, string description = "An entity")
    {
        return new Diagnostic(UnmodifiedId, $"No changed were made to {description} with identifier '{id}'.");
    }

    public static Diagnostic Locked(Hrib id, string description = "The entity")
    {
        return new Diagnostic(LockedId, $"{description} is locked.");
    }

    public static Diagnostic InvalidParameterValue(string what, string? parameterArg)
    {
        var error = new Diagnostic(InvalidParameterValueId, $"{what} has an invalid value.");
        if (!string.IsNullOrEmpty(parameterArg))
        {
            error = error.WithArgument(ParameterArgument, parameterArg);
        }

        return error;
    }

    public static Diagnostic InvalidValue(string message)
    {
        return new Diagnostic(InvalidValueId, message);
    }

    public static Diagnostic AlreadyExists(string? message = null)
    {
        return new Diagnostic(AlreadyExistsId, message ?? "Value already exists.");
    }

    public static Diagnostic AlreadyExists(Hrib id, string description = "An entity")
    {
        return new Diagnostic(AlreadyExistsId, $"{description} with identifier '{id}' already exists.");
    }

    public static Diagnostic BadKafeType(string? value)
    {
        return new Diagnostic(BadKafeTypeId, string.IsNullOrEmpty(value)
            ? "Encountered an invalid or empty KAFE type."
            : $"String '{value}' could not be parsed as a KAFE type.");
    }

    public static Diagnostic InvalidMimeType(string value)
    {
        return new Diagnostic(
            id: InvalidMimeTypeId,
            message: $"String '{value}' is not recognized as any known MIME type."
        ).WithArgument(ValueArgument, value);
    }

    public static Diagnostic ShardAnalysisFailure(KafeType shardType)
    {
        return new Diagnostic(
            id: ShardAnalysisFailureId,
            message: $"The provided data cannot be analyzed as a shard of type '{shardType}'."
        ).WithArgument(nameof(shardType), shardType);
    }
}
