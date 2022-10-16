using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("opening_hours", Schema = "lemma")]
public partial class OpeningHour
{
    [Key]
    [Column("issuemaster")]
    public int Issuemaster { get; set; }
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("day")]
    [StringLength(3)]
    public string Day { get; set; } = null!;
    [Column("since")]
    public TimeOnly Since { get; set; }
    [Column("to")]
    public TimeOnly To { get; set; }

    [ForeignKey("Issuemaster")]
    [InverseProperty("OpeningHours")]
    public virtual Issuemaster IssuemasterNavigation { get; set; } = null!;
}
