using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("vacations", Schema = "lemma")]
public partial class Vacation
{
    [Key]
    [Column("issuemaster")]
    public int Issuemaster { get; set; }
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("since", TypeName = "timestamp without time zone")]
    public DateTime Since { get; set; }
    [Column("to", TypeName = "timestamp without time zone")]
    public DateTime To { get; set; }

    [ForeignKey("Issuemaster")]
    [InverseProperty("Vacations")]
    public virtual Issuemaster IssuemasterNavigation { get; set; } = null!;
}
