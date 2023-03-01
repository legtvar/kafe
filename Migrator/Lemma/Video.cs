using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("video", Schema = "lemma")]
public partial class Video
{
    public Video()
    {
        Entries = new HashSet<Entry>();
        Playlistitems = new HashSet<Playlistitem>();
        Videolinks = new HashSet<Videolink>();
        Votes = new HashSet<Vote>();
    }

    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;
    [Column("adddate", TypeName = "timestamp without time zone")]
    public DateTime Adddate { get; set; }
    [Column("public")]
    [StringLength(255)]
    public string Public { get; set; } = null!;
    [Column("dlsize")]
    public int? Dlsize { get; set; }
    [Column("duration")]
    public int? Duration { get; set; }
    [Column("status")]
    [StringLength(255)]
    public string Status { get; set; } = null!;
    [Column("project")]
    public int Project { get; set; }
    [Column("resolutionx")]
    public int? Resolutionx { get; set; }
    [Column("resolutiony")]
    public int? Resolutiony { get; set; }
    [Column("image")]
    [StringLength(255)]
    public string? Image { get; set; }
    [Column("maxstreamsize")]
    [StringLength(255)]
    public string? Maxstreamsize { get; set; }
    [Column("originalsize")]
    public int? Originalsize { get; set; }
    [Column("originalextension")]
    [StringLength(255)]
    public string? Originalextension { get; set; }
    [Column("framerate")]
    public float? Framerate { get; set; }
    [Column("videoformat")]
    [StringLength(255)]
    public string? Videoformat { get; set; }
    [Column("responsibleperson")]
    public int? Responsibleperson { get; set; }
    [Column("uploadpass")]
    [StringLength(255)]
    public string? Uploadpass { get; set; }
    [Column("affirmation")]
    public string? Affirmation { get; set; }
    [Column("par")]
    public float? Par { get; set; }
    [Column("conversionmethod")]
    [StringLength(255)]
    public string? Conversionmethod { get; set; }
    [Column("license")]
    [StringLength(255)]
    public string? License { get; set; }
    [Column("licenseurl")]
    [StringLength(255)]
    public string? Licenseurl { get; set; }
    [Column("publicrequestdate", TypeName = "timestamp without time zone")]
    public DateTime? Publicrequestdate { get; set; }
    [Column("audioformat")]
    [StringLength(255)]
    public string? Audioformat { get; set; }
    [Column("convertdate", TypeName = "timestamp without time zone")]
    public DateTime? Convertdate { get; set; }

    [ForeignKey("Project")]
    [InverseProperty("Videos")]
    public virtual Project ProjectNavigation { get; set; } = null!;
    [ForeignKey("Responsibleperson")]
    [InverseProperty("Videos")]
    public virtual Person? ResponsiblepersonNavigation { get; set; }
    [InverseProperty("VideoNavigation")]
    public virtual ICollection<Entry> Entries { get; set; }
    [InverseProperty("VideoNavigation")]
    public virtual ICollection<Playlistitem> Playlistitems { get; set; }
    [InverseProperty("VideoNavigation")]
    public virtual ICollection<Videolink> Videolinks { get; set; }
    [InverseProperty("VideoNavigation")]
    public virtual ICollection<Vote> Votes { get; set; }
}
