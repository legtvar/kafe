using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma
{
    [Table("wma_property", Schema = "lemma")]
    public partial class WmaProperty
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; } = null!;
        [Column("textvalue")]
        public string? Textvalue { get; set; }
        [Column("charvalue")]
        [StringLength(255)]
        public string? Charvalue { get; set; }
        [Column("intvalue")]
        public int? Intvalue { get; set; }
        [Column("boolvalue")]
        public bool? Boolvalue { get; set; }
    }
}
