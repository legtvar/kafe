using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma
{
    [Table("collections", Schema = "lemma")]
    public partial class Collection
    {
        public Collection()
        {
            Sources = new HashSet<Source>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; } = null!;
        [Column("available")]
        public bool Available { get; set; }

        [InverseProperty("CollectionNavigation")]
        public virtual ICollection<Source> Sources { get; set; }
    }
}
