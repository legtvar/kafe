using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("project__reservation__source", Schema = "lemma")]
public partial class ProjectReservationSource
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("project")]
    public int Project { get; set; }
    [Column("source")]
    public int Source { get; set; }
    [Column("reservation")]
    public int Reservation { get; set; }

    [ForeignKey("Project")]
    [InverseProperty("ProjectReservationSources")]
    public virtual Project ProjectNavigation { get; set; } = null!;
    [ForeignKey("Reservation")]
    [InverseProperty("ProjectReservationSources")]
    public virtual Reservation ReservationNavigation { get; set; } = null!;
    [ForeignKey("Source")]
    [InverseProperty("ProjectReservationSources")]
    public virtual Source SourceNavigation { get; set; } = null!;
}
