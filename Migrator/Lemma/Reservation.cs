using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("reservations", Schema = "lemma")]
public partial class Reservation
{
    public Reservation()
    {
        ProjectReservationSources = new HashSet<ProjectReservationSource>();
        SourcesReservations = new HashSet<SourcesReservation>();
    }

    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("person")]
    public int Person { get; set; }
    [Column("issuemaster")]
    public int Issuemaster { get; set; }
    [Column("since", TypeName = "timestamp without time zone")]
    public DateTime Since { get; set; }
    [Column("to", TypeName = "timestamp without time zone")]
    public DateTime To { get; set; }
    [Column("status")]
    [StringLength(255)]
    public string Status { get; set; } = null!;
    [Column("crownhours")]
    public int Crownhours { get; set; }
    [Column("notifybeforepickup")]
    public int Notifybeforepickup { get; set; }
    [Required]
    [Column("pickupnotified")]
    public bool? Pickupnotified { get; set; }
    [Column("notifybeforeend")]
    public int Notifybeforeend { get; set; }
    [Required]
    [Column("endnotified")]
    public bool? Endnotified { get; set; }

    [ForeignKey("Issuemaster")]
    [InverseProperty("Reservations")]
    public virtual Issuemaster IssuemasterNavigation { get; set; } = null!;
    [ForeignKey("Person")]
    [InverseProperty("Reservations")]
    public virtual Person PersonNavigation { get; set; } = null!;
    [InverseProperty("ReservationNavigation")]
    public virtual ICollection<ProjectReservationSource> ProjectReservationSources { get; set; }
    [InverseProperty("ReservationNavigation")]
    public virtual ICollection<SourcesReservation> SourcesReservations { get; set; }
}
