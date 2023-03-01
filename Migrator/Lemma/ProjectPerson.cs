using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("project__person", Schema = "lemma")]
public partial class ProjectPerson
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("project")]
    public int Project { get; set; }
    [Column("person")]
    public int Person { get; set; }

    [ForeignKey("Person")]
    [InverseProperty("ProjectPeople")]
    public virtual Person PersonNavigation { get; set; } = null!;
    [ForeignKey("Project")]
    [InverseProperty("ProjectPeople")]
    public virtual Project ProjectNavigation { get; set; } = null!;
}
