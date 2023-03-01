using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("bans", Schema = "lemma")]
public partial class Ban
{
    [Key]
    [Column("person")]
    public int Person { get; set; }
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("admin")]
    public int Admin { get; set; }
    [Column("since", TypeName = "timestamp without time zone")]
    public DateTime Since { get; set; }
    [Column("to", TypeName = "timestamp without time zone")]
    public DateTime? To { get; set; }
    [Column("reason")]
    public string? Reason { get; set; }

    [ForeignKey("Admin")]
    [InverseProperty("BanAdminNavigations")]
    public virtual Person AdminNavigation { get; set; } = null!;
    [ForeignKey("Person")]
    [InverseProperty("BanPersonNavigations")]
    public virtual Person PersonNavigation { get; set; } = null!;
}
