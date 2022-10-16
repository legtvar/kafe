using System.ComponentModel.DataAnnotations.Schema;

namespace Kafe.Wma;

public class Video
{
    public int Id { get; set; }
    public string? Name { get; set; }
    [Column("adddate")]
    public DateTimeOffset AddedAt { get; set; }
    public string? Public { get; set; }
    [Column("dlsize")]
    public int DownloadSize { get; set; }
    [Column("duration")]
    public int Duration { get; set; }
    public string? Status { get; set; }
    public Project? Project { get; set; }
    [Column("resolutionx")]
    public int Width { get; set; }
    [Column("resolutiony")]
    public int Height { get; set; }
    public string? Image { get; set; }
    [Column("maxstreamsize")]
    public string? MaxStreamSize { get; set; }
    [Column("originalsize")]
    public int OriginalSize { get; set; }
    [Column("originalextension")]
    public string? OriginalExtension { get; set; }
    public double Framerate { get; set; }
    [Column("videoformat")]
    public string? VideoFormat { get; set; }
    [Column("responsibleperson")]
    public Person? ResponsiblePerson { get; set; }
    [Column("uploadpass")]
    public string? UploadPass { get; set; }
    [Column("affirmation")]
    public string? Affirmation { get; set; }
    public double Par { get; set; }
    [Column("conversionmethod")]
    public string? ConversionMethod { get; set; }
    public string? License { get; set; }
    [Column("licenseurl")]
    public string? LicenseUrl { get; set; }
    [Column("publicrequestdate")]
    public DateTimeOffset PublicRequestDate { get; set; }
    [Column("audioformat")]
    public string? AudioFormat { get; set; }
    [Column("convertdate")]
    public DateTimeOffset ConvertDate { get; set; }
}
