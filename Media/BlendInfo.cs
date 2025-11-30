using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Kafe.Media;

public record BlendInfo(
    string FileExtension = Const.BlendFileExtension,
    string MimeType = Const.BlendMimeType,
    ImmutableArray<PigeonsTestInfo>? Tests = null,
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

public class BlendInfoJsonFormat
{
    public string FileExtension { get; set; }
    public string MimeType { get; set; }
    public List<PigeonsTestInfoJsonFormat>? Tests { get; set; }
    public string? Error { get; set; }

    public BlendInfo ToBlendInfo()
    {
        return new BlendInfo(
            FileExtension,
            MimeType,
            Tests != null
                ? Tests.Select(dto => dto.ToPigeonsTestInfo()).ToImmutableArray()
                : ImmutableArray<PigeonsTestInfo>.Empty,
            Error
        );
    }
}