using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma
{
    [Table("appf", Schema = "lemma")]
    public partial class Appf
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("uco")]
        public int Uco { get; set; }
        [Column("code")]
        [StringLength(255)]
        public string Code { get; set; } = null!;
    }
}
