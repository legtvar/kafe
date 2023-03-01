using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("issuemasters", Schema = "lemma")]
public partial class Issuemaster
{
    public Issuemaster()
    {
        OpeningHours = new HashSet<OpeningHour>();
        Reservations = new HashSet<Reservation>();
        Sources = new HashSet<Source>();
        Vacations = new HashSet<Vacation>();
        People = new HashSet<Person>();
    }

    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;
    [Column("room")]
    [StringLength(15)]
    public string Room { get; set; } = null!;

    [InverseProperty("IssuemasterNavigation")]
    public virtual ICollection<OpeningHour> OpeningHours { get; set; }
    [InverseProperty("IssuemasterNavigation")]
    public virtual ICollection<Reservation> Reservations { get; set; }
    [InverseProperty("IssuemasterNavigation")]
    public virtual ICollection<Source> Sources { get; set; }
    [InverseProperty("IssuemasterNavigation")]
    public virtual ICollection<Vacation> Vacations { get; set; }

    [ForeignKey("Issuemaster")]
    [InverseProperty("Issuemasters")]
    public virtual ICollection<Person> People { get; set; }
}
