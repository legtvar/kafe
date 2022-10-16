using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma
{
    [Table("emaillog", Schema = "lemma")]
    public partial class Emaillog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("fromaddress", TypeName = "character varying")]
        public string Fromaddress { get; set; } = null!;
        [Column("toaddress", TypeName = "character varying")]
        public string Toaddress { get; set; } = null!;
        [Column("subject", TypeName = "character varying")]
        public string Subject { get; set; } = null!;
        [Column("message", TypeName = "character varying")]
        public string Message { get; set; } = null!;
        [Column("datesent", TypeName = "timestamp without time zone")]
        public DateTime Datesent { get; set; }
        [Column("successful")]
        public bool Successful { get; set; }
    }
}
