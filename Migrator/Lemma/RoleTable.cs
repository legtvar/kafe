using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma
{
    [Table("role_table", Schema = "lemma")]
    public partial class RoleTable
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; } = null!;
        [Column("authorname")]
        [StringLength(255)]
        public string? Authorname { get; set; }
        [Column("project")]
        public int Project { get; set; }
        [Column("authoruco")]
        public int? Authoruco { get; set; }

        [ForeignKey("Authoruco")]
        [InverseProperty("RoleTables")]
        public virtual Author? AuthorucoNavigation { get; set; }
        [ForeignKey("Project")]
        [InverseProperty("RoleTables")]
        public virtual Project ProjectNavigation { get; set; } = null!;
    }
}
