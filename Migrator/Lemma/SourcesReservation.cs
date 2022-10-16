using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma
{
    [Table("sources__reservations", Schema = "lemma")]
    public partial class SourcesReservation
    {
        [Key]
        [Column("source")]
        public int Source { get; set; }
        [Key]
        [Column("reservation")]
        public int Reservation { get; set; }
        [Column("returned", TypeName = "timestamp without time zone")]
        public DateTime? Returned { get; set; }

        [ForeignKey("Reservation")]
        [InverseProperty("SourcesReservations")]
        public virtual Reservation ReservationNavigation { get; set; } = null!;
        [ForeignKey("Source")]
        [InverseProperty("SourcesReservations")]
        public virtual Source SourceNavigation { get; set; } = null!;
    }
}
