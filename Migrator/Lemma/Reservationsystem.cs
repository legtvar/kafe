using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma
{
    [Table("reservationsystem", Schema = "lemma")]
    public partial class Reservationsystem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("daycountinuegraph")]
        public int Daycountinuegraph { get; set; }
        [Column("hoursbeforereminder")]
        public int Hoursbeforereminder { get; set; }
        [Column("countpc")]
        public int Countpc { get; set; }
        [Column("countsourcetype")]
        public int Countsourcetype { get; set; }
        [Column("mailonacceptsubject")]
        [StringLength(255)]
        public string Mailonacceptsubject { get; set; } = null!;
        [Column("mailonaccepttext")]
        public string? Mailonaccepttext { get; set; }
        [Column("mailonrejectsubject")]
        [StringLength(255)]
        public string Mailonrejectsubject { get; set; } = null!;
        [Column("mailonrejecttext")]
        public string? Mailonrejecttext { get; set; }
        [Column("sendmailifaccountrequest")]
        public bool Sendmailifaccountrequest { get; set; }
        [Column("sendmailifreservation")]
        public bool Sendmailifreservation { get; set; }
        [Column("mailonreservationcanceledsubject")]
        [StringLength(255)]
        public string Mailonreservationcanceledsubject { get; set; } = null!;
        [Column("mailonreservationcanceledtext")]
        public string? Mailonreservationcanceledtext { get; set; }
        [Column("mailonsourcedisablesubject")]
        [StringLength(255)]
        public string Mailonsourcedisablesubject { get; set; } = null!;
        [Column("mailonsourcedisabledtext")]
        public string? Mailonsourcedisabledtext { get; set; }
        [Column("mailonreservesubject")]
        [StringLength(255)]
        public string Mailonreservesubject { get; set; } = null!;
        [Column("mailonreservetext")]
        public string? Mailonreservetext { get; set; }
        [Column("mailonaccountrequestsubject")]
        [StringLength(255)]
        public string Mailonaccountrequestsubject { get; set; } = null!;
        [Column("mailonaccountrequesttext")]
        public string? Mailonaccountrequesttext { get; set; }
        [Column("mailonuserdegradedsubject")]
        [StringLength(255)]
        public string Mailonuserdegradedsubject { get; set; } = null!;
        [Column("mailonuserdegradedtext")]
        public string? Mailonuserdegradedtext { get; set; }
        [Column("mailonreservationcanceledbyusersubject")]
        [StringLength(255)]
        public string Mailonreservationcanceledbyusersubject { get; set; } = null!;
        [Column("mailonreservationcanceledbyusertext")]
        public string? Mailonreservationcanceledbyusertext { get; set; }
        [Column("mailonunreservesubject")]
        [StringLength(255)]
        public string Mailonunreservesubject { get; set; } = null!;
        [Column("mailonunreservetext")]
        public string? Mailonunreservetext { get; set; }
        [Column("maxsourcesinreservation")]
        public int Maxsourcesinreservation { get; set; }
        [Column("maxdaysofreservation")]
        public int Maxdaysofreservation { get; set; }
        [Column("maxcrownhours")]
        public long Maxcrownhours { get; set; }
        [Column("mailonbigreservationto")]
        [StringLength(30)]
        public string Mailonbigreservationto { get; set; } = null!;
        [Column("mailonbigreservationsubject")]
        [StringLength(100)]
        public string Mailonbigreservationsubject { get; set; } = null!;
        [Column("weburl")]
        [StringLength(255)]
        public string Weburl { get; set; } = null!;
    }
}
