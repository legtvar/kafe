using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("source_types", Schema = "lemma")]
public partial class SourceType
{
    public SourceType()
    {
        Sources = new HashSet<Source>();
    }

    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;
    [Column("permission_category")]
    public int PermissionCategory { get; set; }
    [Column("position")]
    public int? Position { get; set; }

    [ForeignKey("PermissionCategory")]
    [InverseProperty("SourceTypes")]
    public virtual PermissionCategory PermissionCategoryNavigation { get; set; } = null!;
    [InverseProperty("TypeNavigation")]
    public virtual ICollection<Source> Sources { get; set; }
}
