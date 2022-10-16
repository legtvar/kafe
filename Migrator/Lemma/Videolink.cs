using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("videolink", Schema = "lemma")]
public partial class Videolink
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;
    [Column("video")]
    public int Video { get; set; }
    [Column("expirationtime", TypeName = "timestamp without time zone")]
    public DateTime? Expirationtime { get; set; }
    [Column("streamsize")]
    [StringLength(255)]
    public string Streamsize { get; set; } = null!;

    [ForeignKey("Video")]
    [InverseProperty("Videolinks")]
    public virtual Video VideoNavigation { get; set; } = null!;
}
