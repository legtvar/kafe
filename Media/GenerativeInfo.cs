namespace Kafe.Media;

public record GenerativeInfo(
    string FileExtension = Const.ZipFileExtension,
    string MimeType = Const.ZipMimeType,
    bool IsCorrupted = false
)
{
    public static GenerativeInfo Invalid { get; } = new(
        FileExtension: Const.InvalidFileExtension,
        MimeType: Const.InvalidMimeType,
        IsCorrupted: true
    );
}
