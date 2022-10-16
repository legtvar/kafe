using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma
{
    [Table("stateholidays", Schema = "lemma")]
    [Index("Date", Name = "stateholiday_date_unique", IsUnique = true)]
    [Index("Name", Name = "stateholiday_name_unique", IsUnique = true)]
    public partial class Stateholiday
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name", TypeName = "character varying")]
        public string Name { get; set; } = null!;
        [Column("date")]
        public DateOnly Date { get; set; }
    }
}
