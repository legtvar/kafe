using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Kafe.Mate;

public record BlendInfo(
    string FileExtension = Const.BlendFileExtension,
    string MimeType = Const.BlendMimeType,
    ImmutableArray<PigeonsTestInfo>? Tests = null,
    string? Error = null
): IShardPayload
{
    public static string Moniker => "blend";

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Blender scene"),
        (Const.CzechCulture, "Blender scéna")
    );

    public static BlendInfo CreateInvalid(string? errorMessage = null)
    {
        return new BlendInfo(
            FileExtension: Const.InvalidFileExtension,
            MimeType: Const.InvalidMimeType,
            Tests: null,
            Error: errorMessage ?? "This BlendInfo is Invalid."
        );
    }
}
