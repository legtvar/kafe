using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma
{
    [Table("variables", Schema = "lemma")]
    [Index("Variablename", Name = "variables_unique", IsUnique = true)]
    public partial class Variable
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("variablename", TypeName = "character varying")]
        public string Variablename { get; set; } = null!;
        [Column("variablevalue", TypeName = "character varying")]
        public string? Variablevalue { get; set; }
    }
}
