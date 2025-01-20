namespace Kafe.Media;

public record BlendInfo(
    string FileExtension,
    string MimeType,
    string? Error = null
){}