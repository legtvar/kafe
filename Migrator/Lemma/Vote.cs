using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("vote", Schema = "lemma")]
public partial class Vote
{
    [Column("person")]
    public int? Person { get; set; }
    [Column("video")]
    public int Video { get; set; }
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("time")]
    public int Time { get; set; }
    [Column("ip")]
    [StringLength(255)]
    public string Ip { get; set; } = null!;

    [ForeignKey("Person")]
    [InverseProperty("Votes")]
    public virtual Person? PersonNavigation { get; set; }
    [ForeignKey("Video")]
    [InverseProperty("Votes")]
    public virtual Video VideoNavigation { get; set; } = null!;
}
