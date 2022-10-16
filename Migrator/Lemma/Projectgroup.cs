using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma
{
    [Table("projectgroup", Schema = "lemma")]
    public partial class Projectgroup
    {
        public Projectgroup()
        {
            Projects = new HashSet<Project>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; } = null!;
        [Column("position")]
        public int? Position { get; set; }

        [InverseProperty("GroupNavigation")]
        public virtual ICollection<Project> Projects { get; set; }
    }
}
