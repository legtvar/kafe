using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma
{
    [Table("author", Schema = "lemma")]
    public partial class Author
    {
        public Author()
        {
            RoleTables = new HashSet<RoleTable>();
        }

        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; } = null!;
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [InverseProperty("AuthorucoNavigation")]
        public virtual ICollection<RoleTable> RoleTables { get; set; }
    }
}
