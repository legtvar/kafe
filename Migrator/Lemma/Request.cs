using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("requests", Schema = "lemma")]
public partial class Request
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("type", TypeName = "character varying")]
    public string Type { get; set; } = null!;
    [Column("value", TypeName = "character varying")]
    public string Value { get; set; } = null!;
    [Column("reason")]
    public string? Reason { get; set; }
    [Column("status", TypeName = "character varying")]
    public string Status { get; set; } = null!;
    [Column("requestedby")]
    public int Requestedby { get; set; }
    [Column("daterequested", TypeName = "timestamp without time zone")]
    public DateTime Daterequested { get; set; }
    [Column("modifiedby")]
    public int? Modifiedby { get; set; }
    [Column("datemodified", TypeName = "timestamp without time zone")]
    public DateTime? Datemodified { get; set; }
    [Column("rejectionreason", TypeName = "character varying")]
    public string? Rejectionreason { get; set; }

    [ForeignKey("Modifiedby")]
    [InverseProperty("RequestModifiedbyNavigations")]
    public virtual Person? ModifiedbyNavigation { get; set; }
    [ForeignKey("Requestedby")]
    [InverseProperty("RequestRequestedbyNavigations")]
    public virtual Person RequestedbyNavigation { get; set; } = null!;
}
