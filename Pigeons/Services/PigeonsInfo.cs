using System.Collections.Immutable;

namespace Pigeons.Services;

public record BlendInfo(
    string FileExtension,
    string MimeType,
    ImmutableArray<PigeonsTestInfo>? Tests,
    string? Error = null
)
{
    public static BlendInfo Invalid(string? errorMessage = null) => new(
        FileExtension: Const.InvalidFileExtension,
        MimeType: Const.InvalidMimeType,
        Tests: null,
        Error: errorMessage ?? "This BlendInfo is Invalid."
    );
}
