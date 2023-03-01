using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("entry", Schema = "lemma")]
public partial class Entry
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("video")]
    public int Video { get; set; }
    [Column("ip")]
    [StringLength(255)]
    public string Ip { get; set; } = null!;
    [Column("useragent")]
    [StringLength(255)]
    public string? Useragent { get; set; }
    [Column("date", TypeName = "timestamp without time zone")]
    public DateTime Date { get; set; }
    [Column("days")]
    public long Days { get; set; }
    [Column("event")]
    [StringLength(255)]
    public string Event { get; set; } = null!;
    [Column("type")]
    [StringLength(255)]
    public string? Type { get; set; }

    [ForeignKey("Video")]
    [InverseProperty("Entries")]
    public virtual Video VideoNavigation { get; set; } = null!;
}
