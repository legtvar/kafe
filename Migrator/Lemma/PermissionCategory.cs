using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("permission_categories", Schema = "lemma")]
public partial class PermissionCategory
{
    public PermissionCategory()
    {
        SourceTypes = new HashSet<SourceType>();
        People = new HashSet<Person>();
    }

    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [InverseProperty("PermissionCategoryNavigation")]
    public virtual ICollection<SourceType> SourceTypes { get; set; }

    [ForeignKey("PermissionCategory")]
    [InverseProperty("PermissionCategories")]
    public virtual ICollection<Person> People { get; set; }
}
