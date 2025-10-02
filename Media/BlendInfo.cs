using System.Collections.Immutable;

namespace Kafe.Media;

public record BlendInfo(
    string FileExtension,
    string MimeType,
    ImmutableArray<BlendTestInfo>? Tests,
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


public record BlenderProcessOutput(
    bool Success,
    string Message = ""
)
{ }